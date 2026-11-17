using TMPro;
using UnityEngine;

namespace EcoSort.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [TextArea(1, 4)]
        [SerializeField] private string spanishText;

        [TextArea(1, 4)]
        [SerializeField] private string englishText;

        private TextMeshProUGUI _tmpText;

        public string SpanishText
        {
            get => spanishText;
            set
            {
                spanishText = value;
                UpdateDisplay(LanguageManager.Instance.CurrentLanguage);
            }
        }

        public string EnglishText
        {
            get => englishText;
            set
            {
                englishText = value;
                UpdateDisplay(LanguageManager.Instance.CurrentLanguage);
            }
        }

        private void Awake()
        {
            _tmpText = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            LanguageManager.OnLanguageChanged += UpdateDisplay;
            if (LanguageManager.Instance != null)
            {
                UpdateDisplay(LanguageManager.Instance.CurrentLanguage);
            }
        }

        private void OnDisable()
        {
            LanguageManager.OnLanguageChanged -= UpdateDisplay;
        }

        public void SetTexts(string spanish, string english)
        {
            spanishText = spanish;
            englishText = english;
            if (LanguageManager.Instance != null)
            {
                UpdateDisplay(LanguageManager.Instance.CurrentLanguage);
            }
        }

        private void UpdateDisplay(Language language)
        {
            if (_tmpText == null)
                _tmpText = GetComponent<TextMeshProUGUI>();

            if (_tmpText == null) return;

            _tmpText.text = language == Language.Spanish ? spanishText : englishText;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_tmpText == null)
                _tmpText = GetComponent<TextMeshProUGUI>();

            if (_tmpText != null && !string.IsNullOrEmpty(spanishText))
            {
                _tmpText.text = spanishText;
            }
        }
#endif
    }
}
