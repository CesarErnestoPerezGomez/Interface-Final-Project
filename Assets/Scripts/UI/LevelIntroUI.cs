using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EcoSort.UI
{
    public class LevelIntroUI : MonoBehaviour
    {
        [Header("Texts (Spanish & English)")]
        [SerializeField] private string badgeSpanish = "TUTORIAL";
        [SerializeField] private string badgeEnglish = "TUTORIAL";

        [SerializeField] private string titleSpanish = "¡Aprendamos los controles!";
        [SerializeField] private string titleEnglish = "Let's learn the controls!";

        [TextArea(2, 4)]
        [SerializeField] private string descriptionSpanish = "Antes de comenzar a separar la basura, practiquemos cómo tomar y soltar un cubo.";
        [TextArea(2, 4)]
        [SerializeField] private string descriptionEnglish = "Before we start sorting waste, let's practice picking up and releasing a cube.";

        [TextArea(3, 6)]
        [SerializeField] private string stepsSpanish = "1. Mantén presionado el botón de agarre (Grip) para tomar el cubo.\n2. Muévelo sobre el área marcada (Zona de entrega).\n3. Suelta el botón de agarre para dejarlo caer.";
        [TextArea(3, 6)]
        [SerializeField] private string stepsEnglish = "1. Hold the grip button to pick up the cube.\n2. Move it over the marked area.\n3. Release the grip button to let go.";

        [SerializeField] private string buttonSpanish = "Aceptar";
        [SerializeField] private string buttonEnglish = "OK";

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI badgeText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI stepsText;
        [SerializeField] private Button acceptButton;
        [SerializeField] private TextMeshProUGUI acceptButtonText;
        [SerializeField] private TextMeshProUGUI retryNoticeText;

        [Header("Events")]
        public UnityEvent onAcceptClicked;

        private string _retryNoticeEs = "";
        private string _retryNoticeEn = "";

        private void Awake()
        {
            if (acceptButton != null)
            {
                acceptButton.onClick.AddListener(HandleAcceptClicked);
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

        public void ConfigureIntro(
            string badgeEs, string badgeEn,
            string titleEs, string titleEn,
            string descEs, string descEn,
            string stepsEs, string stepsEn,
            string btnEs = "Aceptar", string btnEn = "OK")
        {
            badgeSpanish = badgeEs;
            badgeEnglish = badgeEn;
            titleSpanish = titleEs;
            titleEnglish = titleEn;
            descriptionSpanish = descEs;
            descriptionEnglish = descEn;
            stepsSpanish = stepsEs;
            stepsEnglish = stepsEn;
            buttonSpanish = btnEs;
            buttonEnglish = btnEn;

            RefreshUI(LanguageManager.Instance != null ? LanguageManager.Instance.CurrentLanguage : Language.Spanish);
        }

        public void SetRetryNotice(string noticeEs, string noticeEn)
        {
            _retryNoticeEs = noticeEs;
            _retryNoticeEn = noticeEn;
            RefreshUI(LanguageManager.Instance != null ? LanguageManager.Instance.CurrentLanguage : Language.Spanish);
        }

        public void RefreshUI(Language language)
        {
            bool isEs = language == Language.Spanish;

            if (badgeText != null)
                badgeText.text = isEs ? badgeSpanish : badgeEnglish;

            if (titleText != null)
                titleText.text = isEs ? titleSpanish : titleEnglish;

            if (descriptionText != null)
                descriptionText.text = isEs ? descriptionSpanish : descriptionEnglish;

            if (stepsText != null)
                stepsText.text = isEs ? stepsSpanish : stepsEnglish;

            if (acceptButtonText != null)
                acceptButtonText.text = isEs ? buttonSpanish : buttonEnglish;

            if (retryNoticeText != null)
            {
                string notice = isEs ? _retryNoticeEs : _retryNoticeEn;
                retryNoticeText.text = notice;
                retryNoticeText.gameObject.SetActive(!string.IsNullOrEmpty(notice));
            }
        }

        private void HandleAcceptClicked()
        {
            onAcceptClicked?.Invoke();
        }
    }
}
