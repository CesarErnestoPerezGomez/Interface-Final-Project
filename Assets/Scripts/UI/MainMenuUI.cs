using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EcoSort.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button progressButton;
        [SerializeField] private Button languageToggleButton;

        [Header("Language UI")]
        [SerializeField] private TextMeshProUGUI languageButtonText;

        [Header("Scene Navigation")]
        [SerializeField] private string playSceneName = "TutorialLevel";

        [Header("Events")]
        public UnityEvent onPlayClicked;
        public UnityEvent onProgressClicked;

        private void Awake()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonPressed);

            if (progressButton != null)
                progressButton.onClick.AddListener(OnProgressButtonPressed);

            if (languageToggleButton != null)
                languageToggleButton.onClick.AddListener(OnLanguageToggleButtonPressed);
        }

        private void Start()
        {
            VRViewHelper.StabilizeAndCenterVRView(this);
        }

        private void OnEnable()
        {
            LanguageManager.OnLanguageChanged += HandleLanguageChanged;
            if (LanguageManager.Instance != null)
                HandleLanguageChanged(LanguageManager.Instance.CurrentLanguage);
        }

        private void OnDisable()
        {
            LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
        }

        private void HandleLanguageChanged(Language language)
        {
            if (languageButtonText != null)
            {
                // In Spanish, show button to switch to English; in English, show button to switch to Spanish
                languageButtonText.text = language == Language.Spanish ? "Idioma: English" : "Language: Español";
            }
        }

        private void OnPlayButtonPressed()
        {
            onPlayClicked?.Invoke();

            if (!string.IsNullOrEmpty(playSceneName))
            {
                // If scene is in build settings, load it
                if (Application.CanStreamedLevelBeLoaded(playSceneName))
                {
                    SceneManager.LoadScene(playSceneName);
                }
                else
                {
                    Debug.Log($"[MainMenuUI] Play clicked. Scene '{playSceneName}' not in build settings. (Triggered onPlayClicked event)");
                }
            }
        }

        private void OnProgressButtonPressed()
        {
            onProgressClicked?.Invoke();
            Debug.Log("[MainMenuUI] Progress button clicked.");
        }

        private void OnLanguageToggleButtonPressed()
        {
            LanguageManager.Instance.ToggleLanguage();
        }
    }
}
