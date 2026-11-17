using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using EcoSort.UI;

namespace EcoSort.Gameplay
{
    public enum LevelPhase
    {
        IntroFeedback = 0,
        Playing = 1,
        Completed = 2
    }

    public class EcoLevelManager : MonoBehaviour
    {
        public static EcoLevelManager Instance { get; private set; }

        [Header("Level Identity & Progression")]
        [Tooltip("0 = Tutorial, 1..9 = Progressive Levels")]
        [SerializeField] private int levelNumber = 0;
        [SerializeField] private int totalLevels = 9;
        [SerializeField] private string playerName = "Colibrí";
        [SerializeField] private string nextLevelSceneName = "Level1";
        [SerializeField] private string nextGoalSpanish = "Explora una nueva categoría";
        [SerializeField] private string nextGoalEnglish = "Explore a new category";

        [Header("Phase Roots & UI Canvases")]
        [SerializeField] private LevelIntroUI levelIntroUI;
        [SerializeField] private InGameHUDUI inGameHUDUI;
        [SerializeField] private LevelCompletedUI levelCompletedUI;
        [SerializeField] private GameObject gameplay3DRoot;

        [Header("Gameplay References")]
        [SerializeField] private List<EcoBin> levelBins = new List<EcoBin>();
        [SerializeField] private List<EcoTrashItem> levelTrashItems = new List<EcoTrashItem>();

        private LevelPhase _currentPhase = LevelPhase.IntroFeedback;
        private int _sortedCount = 0;
        private int _totalItems = 1;
        private bool _isTransitioning = false;

        public LevelPhase CurrentPhase => _currentPhase;
        public int LevelNumber => levelNumber;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            VRViewHelper.StabilizeAndCenterVRView(this);

            // Auto-collect bins and items inside gameplay3DRoot if lists are empty
            if (gameplay3DRoot != null)
            {
                if (levelBins == null || levelBins.Count == 0)
                {
                    levelBins = new List<EcoBin>(gameplay3DRoot.GetComponentsInChildren<EcoBin>(true));
                }
                if (levelTrashItems == null || levelTrashItems.Count == 0)
                {
                    levelTrashItems = new List<EcoTrashItem>(gameplay3DRoot.GetComponentsInChildren<EcoTrashItem>(true));
                }
            }

            _totalItems = Mathf.Max(1, levelTrashItems.Count);
            _sortedCount = 0;

            // Wire UI events
            if (levelIntroUI != null)
            {
                levelIntroUI.onAcceptClicked.RemoveListener(StartGameplayPhase);
                levelIntroUI.onAcceptClicked.AddListener(StartGameplayPhase);
            }

            if (inGameHUDUI != null)
            {
                inGameHUDUI.onHintClicked.RemoveListener(UseHint);
                inGameHUDUI.onHintClicked.AddListener(UseHint);
            }

            if (levelCompletedUI != null)
            {
                levelCompletedUI.onRetryClicked.RemoveListener(HandleManualRetry);
                levelCompletedUI.onRetryClicked.AddListener(HandleManualRetry);
            }

            EnterIntroPhase();
        }

        public void EnterIntroPhase()
        {
            _currentPhase = LevelPhase.IntroFeedback;
            _isTransitioning = false;

            int attempts = EcoSessionTracker.GetAttempts(levelNumber);
            if (levelIntroUI != null)
            {
                levelIntroUI.gameObject.SetActive(true);
                if (EcoSessionTracker.WasRestartedFromMistake(levelNumber) && attempts > 1)
                {
                    string lastEs = EcoSessionTracker.GetLastMistakeEs(levelNumber);
                    string lastEn = EcoSessionTracker.GetLastMistakeEn(levelNumber);
                    levelIntroUI.SetRetryNotice(
                        $"Intento #{attempts}: {lastEs}",
                        $"Attempt #{attempts}: {lastEn}"
                    );
                }
                else
                {
                    levelIntroUI.SetRetryNotice("", "");
                }
            }

            if (inGameHUDUI != null)
                inGameHUDUI.gameObject.SetActive(false);

            if (levelCompletedUI != null)
                levelCompletedUI.gameObject.SetActive(false);

            if (gameplay3DRoot != null)
                gameplay3DRoot.SetActive(true);

            foreach (var item in levelTrashItems)
            {
                if (item != null)
                    item.gameObject.SetActive(false);
            }
        }

        public void StartGameplayPhase()
        {
            _currentPhase = LevelPhase.Playing;
            _isTransitioning = false;
            _sortedCount = 0;

            if (levelIntroUI != null)
                levelIntroUI.gameObject.SetActive(false);

            if (levelCompletedUI != null)
                levelCompletedUI.gameObject.SetActive(false);

            if (gameplay3DRoot != null)
                gameplay3DRoot.SetActive(true);

            foreach (var item in levelTrashItems)
            {
                if (item != null)
                {
                    item.ResetToSpawn();
                }
            }

            ClearAllBinHighlights();

            if (inGameHUDUI != null)
            {
                inGameHUDUI.gameObject.SetActive(true);
                inGameHUDUI.UpdateProgress(0, _totalItems);
            }
        }

        public void OnItemSelected(EcoTrashItem item)
        {
            if (_currentPhase != LevelPhase.Playing || item == null || inGameHUDUI == null) return;

            inGameHUDUI.ShowFeedback(
                $"Seleccionado: {item.ItemNameSpanish} — Llévalo o haz clic en su bote correspondiente.",
                $"Selected: {item.ItemNameEnglish} — Move it or click on its matching bin.",
                true,
                3.0f
            );
        }

        public void OnItemDroppedInBin(EcoTrashItem item, EcoBin bin)
        {
            if (_currentPhase != LevelPhase.Playing || _isTransitioning || item == null || bin == null || item.IsSorted)
                return;

            ClearAllBinHighlights();

            // Check if the item matches the bin's category
            if (item.Category == bin.AcceptedCategory)
            {
                // Correct!
                item.MarkAsSortedAndAbsorb(bin.DropPoint);
                _sortedCount++;

                if (inGameHUDUI != null)
                {
                    inGameHUDUI.UpdateProgress(_sortedCount, _totalItems);

                    if (levelNumber <= 0)
                    {
                        inGameHUDUI.ShowFeedback(
                            "¡Muy bien! Soltaste el cubo en la zona de entrega.",
                            "Great job! You released the cube in the drop zone.",
                            true,
                            2.0f
                        );
                    }
                    else
                    {
                        inGameHUDUI.ShowFeedback(
                            $"¡Correcto! {item.ItemNameSpanish} va en {WasteCategoryInfo.GetBinNameEs(bin.AcceptedCategory)}.",
                            $"Correct! {item.ItemNameEnglish} goes in the {WasteCategoryInfo.GetBinNameEn(bin.AcceptedCategory)}.",
                            true,
                            2.0f
                        );
                    }
                }

                if (_sortedCount >= _totalItems)
                {
                    _isTransitioning = true;
                    StartCoroutine(CompleteLevelRoutine());
                }
            }
            else
            {
                // Mistake! Per project scope: "si el jugador se equivoca en algún punto el nivel se reinicia"
                _isTransitioning = true;

                string errEs = $"¡Ups! {item.ItemNameSpanish} no va en {WasteCategoryInfo.GetBinNameEs(bin.AcceptedCategory)}. ¡Intentémoslo de nuevo!";
                string errEn = $"Oops! {item.ItemNameEnglish} doesn't go in the {WasteCategoryInfo.GetBinNameEn(bin.AcceptedCategory)}. Let's try again!";

                EcoSessionTracker.RecordMistakeAndPrepareRestart(levelNumber, errEs, errEn);

                if (inGameHUDUI != null)
                {
                    inGameHUDUI.ShowFeedback(
                        $"{errEs} (Reiniciando nivel...)",
                        $"{errEn} (Restarting level...)",
                        false,
                        2.5f
                    );
                }

                StartCoroutine(RestartLevelOnMistakeRoutine());
            }
        }

        public void UseHint()
        {
            if (_currentPhase != LevelPhase.Playing || _isTransitioning) return;

            // Find either the currently selected item or the first unsorted item
            EcoTrashItem targetItem = EcoTrashItem.CurrentlySelectedItem;
            if (targetItem == null || targetItem.IsSorted)
            {
                targetItem = levelTrashItems.Find(i => i != null && !i.IsSorted);
            }

            if (targetItem == null) return;

            EcoSessionTracker.AddHintUsed(levelNumber);
            ClearAllBinHighlights();

            EcoBin matchingBin = levelBins.Find(b => b != null && b.AcceptedCategory == targetItem.Category);
            if (matchingBin != null)
            {
                matchingBin.SetHintHighlight(true);
            }

            if (inGameHUDUI != null)
            {
                string binEs = WasteCategoryInfo.GetBinNameEs(targetItem.Category);
                string binEn = WasteCategoryInfo.GetBinNameEn(targetItem.Category);
                inGameHUDUI.ShowFeedback(
                    $"Pista: Pon '{targetItem.ItemNameSpanish}' en {binEs}.",
                    $"Hint: Put '{targetItem.ItemNameEnglish}' in the {binEn}.",
                    true,
                    3.5f
                );
            }
        }

        private void ClearAllBinHighlights()
        {
            foreach (var b in levelBins)
            {
                if (b != null)
                    b.SetHintHighlight(false);
            }
        }

        private IEnumerator RestartLevelOnMistakeRoutine()
        {
            yield return new WaitForSeconds(1.25f);

            Scene active = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(active.name) && Application.CanStreamedLevelBeLoaded(active.name))
            {
                SceneManager.LoadScene(active.name);
            }
            else
            {
                // Fallback in-place reset to Intro Phase
                EnterIntroPhase();
            }
        }

        private IEnumerator CompleteLevelRoutine()
        {
            yield return new WaitForSeconds(0.75f);
            EnterCompletedPhase();
        }

        public void EnterCompletedPhase()
        {
            _currentPhase = LevelPhase.Completed;
            _isTransitioning = false;

            if (levelIntroUI != null)
                levelIntroUI.gameObject.SetActive(false);

            if (inGameHUDUI != null)
                inGameHUDUI.gameObject.SetActive(false);

            if (gameplay3DRoot != null)
                gameplay3DRoot.SetActive(true);

            foreach (var item in levelTrashItems)
            {
                if (item != null)
                    item.gameObject.SetActive(false);
            }

            int attempts = EcoSessionTracker.GetAttempts(levelNumber);
            int hints = EcoSessionTracker.GetAccumulatedHints(levelNumber);
            bool firstAttempt = (attempts == 1);

            int stars = 3;
            if (attempts == 1 && hints == 0)
                stars = 3;
            else if (attempts <= 2 && hints <= 1)
                stars = 2;
            else
                stars = 1;

            if (levelCompletedUI != null)
            {
                levelCompletedUI.gameObject.SetActive(true);
                levelCompletedUI.SetNextLevelScene(nextLevelSceneName);
                levelCompletedUI.SetBinMasteries(EcoSessionTracker.GetBinMasteriesForLevel(levelNumber));
                levelCompletedUI.SetLevelResult(
                    levelNumber,
                    totalLevels,
                    playerName,
                    stars,
                    attempts,
                    hints,
                    firstAttempt,
                    nextGoalSpanish,
                    nextGoalEnglish
                );
            }

            // Reset session tracker for this level now that it's completed
            EcoSessionTracker.ClearLevelSession(levelNumber);
        }

        private void HandleManualRetry()
        {
            EcoSessionTracker.ClearLevelSession(levelNumber);
        }
    }
}
