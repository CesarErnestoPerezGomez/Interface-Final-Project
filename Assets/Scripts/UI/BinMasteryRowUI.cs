using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EcoSort.UI
{
    public class BinMasteryRowUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image dotImage;
        [SerializeField] private TextMeshProUGUI binNameText;
        [SerializeField] private Slider progressBarSlider;
        [SerializeField] private Image progressBarFill;
        [SerializeField] private TextMeshProUGUI percentOrStatusText;

        [Header("Data")]
        [SerializeField] private string spanishName = "Verde";
        [SerializeField] private string englishName = "Green";
        [SerializeField] private Color themeColor = new Color(0.18f, 0.64f, 0.36f);
        [Range(-1, 100)]
        [SerializeField] private int masteryPercent = 100; // -1 for "Pronto"

        public void Setup(string esName, string enName, Color color, int percent)
        {
            spanishName = esName;
            englishName = enName;
            themeColor = color;
            masteryPercent = percent;
            Refresh(LanguageManager.Instance != null && LanguageManager.Instance.CurrentLanguage == Language.Spanish);
        }

        public void SetMasteryPercent(int percent)
        {
            masteryPercent = Mathf.Clamp(percent, -1, 100);
            Refresh(LanguageManager.Instance == null || LanguageManager.Instance.CurrentLanguage == Language.Spanish);
        }

        public void Refresh(bool isSpanish)
        {
            if (dotImage != null)
                dotImage.color = themeColor;

            if (binNameText != null)
                binNameText.text = isSpanish ? spanishName : englishName;

            bool isAvailable = masteryPercent >= 0;

            if (progressBarSlider != null)
            {
                progressBarSlider.minValue = 0f;
                progressBarSlider.maxValue = 100f;
                progressBarSlider.value = isAvailable ? masteryPercent : 0f;
            }

            if (progressBarFill != null)
            {
                progressBarFill.color = isAvailable ? themeColor : new Color(0.85f, 0.90f, 0.87f, 0.5f);
            }

            if (percentOrStatusText != null)
            {
                if (isAvailable)
                {
                    percentOrStatusText.text = $"{masteryPercent}%";
                    percentOrStatusText.color = new Color(0.08f, 0.23f, 0.16f); // dark
                }
                else
                {
                    percentOrStatusText.text = isSpanish ? "Pronto" : "Soon";
                    percentOrStatusText.color = new Color(0.46f, 0.56f, 0.51f); // muted
                }
            }
        }
    }
}
