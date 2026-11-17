using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EcoSort.UI
{
    [Serializable]
    public class BinMasteryData
    {
        public string binKey; // "Green", "Grey", "Red", "Blue", "Yellow", "Orange"
        public string spanishName;
        public string englishName;
        public Color dotColor;
        [Range(-1, 100)]
        public int masteryPercent; // -1 means "Pronto" / "Soon"
    }

    public class LevelCompletedUI : MonoBehaviour
    {
        [Header("Level Data")]
        [SerializeField] private int levelNumber = 3;
        [SerializeField] private int totalLevels = 9;
        [SerializeField] private string playerName = "Colibrí";
        [Range(0, 3)]
        [SerializeField] private int starRating = 2;
        [SerializeField] private int attemptsCount = 2;
        [SerializeField] private int hintsUsedCount = 1;
        [SerializeField] private bool isFirstAttempt = false;
        [SerializeField] private string nextGoalSpanish = "Practica el bote rojo";
        [SerializeField] private string nextGoalEnglish = "Practice the red bin";
        [SerializeField] private string customSubheaderSpanish = "";
        [SerializeField] private string customSubheaderEnglish = "";

        [Header("Header UI")]
        [SerializeField] private TextMeshProUGUI headerTitleText;
        [SerializeField] private TextMeshProUGUI subheaderText;

        [Header("Stars")]
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite starFilledSprite;
        [SerializeField] private Sprite starEmptySprite;

        [Header("Stat Cards")]
        [SerializeField] private TextMeshProUGUI attemptsValueText;
        [SerializeField] private TextMeshProUGUI hintsValueText;
        [SerializeField] private TextMeshProUGUI firstAttemptValueText;

        [Header("Level Progress Indicator")]
        [SerializeField] private TextMeshProUGUI levelProgressLabel;
        [SerializeField] private Transform levelDotsContainer;
        [SerializeField] private Sprite circleFilledSprite;
        [SerializeField] private Sprite circleOutlineSprite;

        [Header("Right Panel - Bin Mastery")]
        [SerializeField] private TextMeshProUGUI binMasteryHeader;
        [SerializeField] private Transform binRowsContainer;
        [SerializeField] private List<BinMasteryRowUI> binRows = new List<BinMasteryRowUI>();

        [Header("Next Goal Card")]
        [SerializeField] private TextMeshProUGUI nextGoalTitle;
        [SerializeField] private TextMeshProUGUI nextGoalDescription;

        [Header("Buttons")]
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;

        [Header("Scene Navigation Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string nextLevelSceneName = "";

        [Header("Events")]
        public UnityEvent onNextLevelClicked;
        public UnityEvent onRetryClicked;
        public UnityEvent onMenuClicked;

        private void Awake()
        {
            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(OnNextLevelPressed);

            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryPressed);

            if (menuButton != null)
                menuButton.onClick.AddListener(OnMenuPressed);
        }

        private void Start()
        {
            VRViewHelper.StabilizeAndCenterVRView(this);
        }

        private void OnEnable()
        {
            LanguageManager.OnLanguageChanged += RefreshUI;
            RefreshUI(LanguageManager.Instance != null ? LanguageManager.Instance.CurrentLanguage : Language.Spanish);
        }

        private void OnDisable()
        {
            LanguageManager.OnLanguageChanged -= RefreshUI;
        }

        public void SetLevelResult(int level, int total, string player, int stars, int attempts, int hints, bool firstTry, string goalEs, string goalEn, string subEs = "", string subEn = "")
        {
            levelNumber = level;
            totalLevels = total;
            playerName = player;
            starRating = Mathf.Clamp(stars, 0, 3);
            attemptsCount = attempts;
            hintsUsedCount = hints;
            isFirstAttempt = firstTry;
            nextGoalSpanish = goalEs;
            nextGoalEnglish = goalEn;
            customSubheaderSpanish = subEs;
            customSubheaderEnglish = subEn;

            RefreshUI(LanguageManager.Instance != null ? LanguageManager.Instance.CurrentLanguage : Language.Spanish);
        }

        public void SetBinMasteries(int[] percents)
        {
            if (percents == null || binRows == null) return;
            for (int i = 0; i < binRows.Count && i < percents.Length; i++)
            {
                if (binRows[i] != null)
                {
                    binRows[i].SetMasteryPercent(percents[i]);
                }
            }
        }

        public void SetNextLevelScene(string sceneName)
        {
            nextLevelSceneName = sceneName;
        }

        public void RefreshUI(Language language)
        {
            bool isEs = language == Language.Spanish;

            // 1. Header
            if (headerTitleText != null)
            {
                if (levelNumber <= 0)
                {
                    headerTitleText.text = isEs ? "¡Tutorial completado!" : "Tutorial completed";
                }
                else
                {
                    headerTitleText.text = isEs 
                        ? $"¡Nivel {levelNumber} completado!" 
                        : $"Level {levelNumber} Completed!";
                }
            }

            // 2. Subheader
            if (subheaderText != null)
            {
                if (!string.IsNullOrEmpty(customSubheaderSpanish) || !string.IsNullOrEmpty(customSubheaderEnglish))
                {
                    subheaderText.text = isEs ? customSubheaderSpanish : customSubheaderEnglish;
                }
                else if (levelNumber <= 0)
                {
                    subheaderText.text = isEs
                        ? "Ahora ya sabes cómo tomar y soltar objetos."
                        : "Now you know how to pick up and release objects.";
                }
                else
                {
                    string hintPartEs = hintsUsedCount == 0 ? "sin pistas" : (hintsUsedCount == 1 ? "con una pista" : $"con {hintsUsedCount} pistas");
                    string hintPartEn = hintsUsedCount == 0 ? "with no hints" : (hintsUsedCount == 1 ? "with one hint" : $"with {hintsUsedCount} hints");
                    subheaderText.text = isEs
                        ? $"¡Muy bien, {playerName}! Lo lograste {hintPartEs}."
                        : $"Great job, {playerName}! You did it {hintPartEn}.";
                }
            }

            // 3. Stars
            if (starImages != null && starFilledSprite != null && starEmptySprite != null)
            {
                for (int i = 0; i < starImages.Length; i++)
                {
                    if (starImages[i] != null)
                    {
                        starImages[i].sprite = i < starRating ? starFilledSprite : starEmptySprite;
                    }
                }
            }

            // 4. Stats
            if (attemptsValueText != null) attemptsValueText.text = attemptsCount.ToString();
            if (hintsValueText != null) hintsValueText.text = hintsUsedCount.ToString();
            if (firstAttemptValueText != null)
            {
                firstAttemptValueText.text = isFirstAttempt 
                    ? (isEs ? "Sí" : "Yes") 
                    : "No";
            }

            // 5. Level Progression tracker
            if (levelProgressLabel != null)
            {
                if (levelNumber <= 0)
                {
                    levelProgressLabel.text = isEs
                        ? "Progreso: tutorial completado"
                        : "Progress: tutorial completed";
                }
                else
                {
                    levelProgressLabel.text = isEs 
                        ? $"Nivel {levelNumber} de {totalLevels}" 
                        : $"Level {levelNumber} of {totalLevels}";
                }
            }

            UpdateLevelDots();

            // 6. Bin Mastery Rows
            if (binMasteryHeader != null)
            {
                binMasteryHeader.text = isEs ? "Tu dominio por bote" : "Your mastery per bin";
            }

            foreach (var row in binRows)
            {
                if (row != null)
                {
                    row.Refresh(isEs);
                }
            }

            // 7. Next goal card
            if (nextGoalTitle != null)
            {
                nextGoalTitle.text = isEs ? "Siguiente meta" : "Next goal";
            }
            if (nextGoalDescription != null)
            {
                nextGoalDescription.text = isEs ? nextGoalSpanish : nextGoalEnglish;
            }
        }

        private void UpdateLevelDots()
        {
            if (levelDotsContainer == null) return;

            int count = levelDotsContainer.childCount;
            for (int i = 0; i < count; i++)
            {
                Transform child = levelDotsContainer.GetChild(i);
                int lvl = i + 1;
                Image img = child.GetComponent<Image>();
                TextMeshProUGUI txt = child.GetComponentInChildren<TextMeshProUGUI>();

                if (txt != null) txt.text = lvl.ToString();

                if (lvl <= levelNumber)
                {
                    // Completed level: filled green with white text
                    if (img != null && circleFilledSprite != null)
                    {
                        img.sprite = circleFilledSprite;
                        img.color = new Color(0.14f, 0.45f, 0.28f, 1f); // #247348
                    }
                    if (txt != null) txt.color = Color.white;
                }
                else if (lvl == levelNumber + 1)
                {
                    // Next / current level: outline green
                    if (img != null && circleOutlineSprite != null)
                    {
                        img.sprite = circleOutlineSprite;
                        img.color = Color.white;
                    }
                    if (txt != null) txt.color = new Color(0.14f, 0.45f, 0.28f, 1f);
                }
                else
                {
                    // Upcoming level: light muted grey
                    if (img != null && circleFilledSprite != null)
                    {
                        img.sprite = circleFilledSprite;
                        img.color = new Color(0.86f, 0.91f, 0.88f, 1f); // #DCE8E0
                    }
                    if (txt != null) txt.color = new Color(0.34f, 0.45f, 0.39f, 1f); // #567363
                }
            }
        }

        private void OnNextLevelPressed()
        {
            onNextLevelClicked?.Invoke();
            if (!string.IsNullOrEmpty(nextLevelSceneName) && Application.CanStreamedLevelBeLoaded(nextLevelSceneName))
            {
                SceneManager.LoadScene(nextLevelSceneName);
            }
            else
            {
                Debug.Log("[LevelCompletedUI] Next Level clicked.");
            }
        }

        private void OnRetryPressed()
        {
            onRetryClicked?.Invoke();
            Scene current = SceneManager.GetActiveScene();
            SceneManager.LoadScene(current.name);
        }

        private void OnMenuPressed()
        {
            onMenuClicked?.Invoke();
            if (!string.IsNullOrEmpty(mainMenuSceneName) && Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
            else
            {
                Debug.Log($"[LevelCompletedUI] Menu clicked. Scene '{mainMenuSceneName}' not loaded.");
            }
        }
    }
}
