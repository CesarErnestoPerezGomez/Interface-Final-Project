using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EcoSort.UI
{
    public class InGameHUDUI : MonoBehaviour
    {
        [Header("Localized Texts")]
        [SerializeField] private string taskTitleSpanish = "Toma el cubo";
        [SerializeField] private string taskTitleEnglish = "Pick up the cube";

        [SerializeField] private string taskSubtitleSpanish = "Mueve el cubo hacia el área marcada y suéltalo.";
        [SerializeField] private string taskSubtitleEnglish = "Move the cube to the marked area, then release it.";

        [SerializeField] private string bottomPromptSpanish = "Usa el botón de agarre (Grip) para tomar y soltar el cubo.";
        [SerializeField] private string bottomPromptEnglish = "Use the grip button to pick up and release the cube.";

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI taskTitleText;
        [SerializeField] private TextMeshProUGUI taskSubtitleText;
        [SerializeField] private TextMeshProUGUI progressBadgeText;
        [SerializeField] private TextMeshProUGUI attemptBadgeText;
        [SerializeField] private TextMeshProUGUI bottomPromptText;
        [SerializeField] private Button hintButton;
        [SerializeField] private TextMeshProUGUI hintButtonText;
        [SerializeField] private GameObject feedbackBannerObj;
        [SerializeField] private Image feedbackBannerBg;
        [SerializeField] private TextMeshProUGUI feedbackBannerText;

        [Header("Events")]
        public UnityEvent onHintClicked;

        private int _sortedCount = 0;
        private int _totalToSort = 1;
        private int _attemptNumber = 1;
        private Coroutine _feedbackCoroutine;

        private void Awake()
        {
            if (hintButton != null)
            {
                hintButton.onClick.AddListener(() => onHintClicked?.Invoke());
            }
            if (feedbackBannerObj != null)
            {
                feedbackBannerObj.SetActive(false);
            }
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

        public void ConfigureHUD(
            string titleEs, string titleEn,
            string subEs, string subEn,
            string bottomEs, string bottomEn,
            int sorted, int total, int attempt)
        {
            taskTitleSpanish = titleEs;
            taskTitleEnglish = titleEn;
            taskSubtitleSpanish = subEs;
            taskSubtitleEnglish = subEn;
            bottomPromptSpanish = bottomEs;
            bottomPromptEnglish = bottomEn;
            _sortedCount = sorted;
            _totalToSort = Mathf.Max(1, total);
            _attemptNumber = Mathf.Max(1, attempt);

            RefreshUI(LanguageManager.Instance != null ? LanguageManager.Instance.CurrentLanguage : Language.Spanish);
        }

        public void UpdateProgress(int sorted, int total)
        {
            _sortedCount = sorted;
            _totalToSort = Mathf.Max(1, total);
            RefreshUI(LanguageManager.Instance != null ? LanguageManager.Instance.CurrentLanguage : Language.Spanish);
        }

        public void ShowFeedback(string messageEs, string messageEn, bool isSuccess, float duration = 2.0f)
        {
            if (feedbackBannerObj == null || feedbackBannerText == null) return;

            bool isEs = LanguageManager.Instance == null || LanguageManager.Instance.CurrentLanguage == Language.Spanish;
            feedbackBannerText.text = isEs ? messageEs : messageEn;

            if (feedbackBannerBg != null)
            {
                feedbackBannerBg.color = isSuccess
                    ? new Color(0.14f, 0.52f, 0.30f, 0.96f) // Cozy leaf green
                    : new Color(0.82f, 0.22f, 0.22f, 0.96f); // Friendly coral red
            }

            feedbackBannerObj.SetActive(true);

            if (_feedbackCoroutine != null)
                StopCoroutine(_feedbackCoroutine);

            if (duration > 0f && gameObject.activeInHierarchy)
            {
                _feedbackCoroutine = StartCoroutine(HideFeedbackAfter(duration));
            }
        }

        private IEnumerator HideFeedbackAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (feedbackBannerObj != null)
                feedbackBannerObj.SetActive(false);
        }

        public void RefreshUI(Language language)
        {
            bool isEs = language == Language.Spanish;

            if (taskTitleText != null)
                taskTitleText.text = isEs ? taskTitleSpanish : taskTitleEnglish;

            if (taskSubtitleText != null)
                taskSubtitleText.text = isEs ? taskSubtitleSpanish : taskSubtitleEnglish;

            if (bottomPromptText != null)
                bottomPromptText.text = isEs ? bottomPromptSpanish : bottomPromptEnglish;

            if (progressBadgeText != null)
            {
                progressBadgeText.text = isEs
                    ? $"Clasificados: {_sortedCount} / {_totalToSort}"
                    : $"Sorted: {_sortedCount} / {_totalToSort}";
            }

            if (attemptBadgeText != null)
            {
                attemptBadgeText.text = isEs
                    ? $"Intento #{_attemptNumber}"
                    : $"Attempt #{_attemptNumber}";
            }

            if (hintButtonText != null)
            {
                hintButtonText.text = isEs ? "Pista (?)" : "Hint (?)";
            }
        }
    }
}
