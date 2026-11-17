using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using EcoSort.UI;

namespace EcoSort.Gameplay
{
    public class EcoBin : MonoBehaviour, IPointerClickHandler
    {
        [Header("Bin Configuration")]
        [SerializeField] private WasteCategory acceptedCategory = WasteCategory.GreenOrganic;
        [SerializeField] private string titleSpanish = "VERDE";
        [SerializeField] private string titleEnglish = "GREEN";
        [SerializeField] private string subtitleSpanish = "Orgánico";
        [SerializeField] private string subtitleEnglish = "Organic";

        [Header("Visual References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private Transform dropTargetPoint;
        [SerializeField] private GameObject hintHighlightRing;

        private Vector3 _baseScale;
        private bool _isHintActive = false;

        public WasteCategory AcceptedCategory => acceptedCategory;
        public string TitleSpanish => titleSpanish;
        public string TitleEnglish => titleEnglish;
        public string SubtitleSpanish => subtitleSpanish;
        public string SubtitleEnglish => subtitleEnglish;

        public Vector3 DropPoint => dropTargetPoint != null ? dropTargetPoint.position : transform.position + Vector3.up * 0.45f;

        private void Awake()
        {
            _baseScale = transform.localScale;
            if (hintHighlightRing != null)
                hintHighlightRing.SetActive(false);
        }

        private void OnEnable()
        {
            LanguageManager.OnLanguageChanged += RefreshLabels;
            RefreshLabels(LanguageManager.Instance != null ? LanguageManager.Instance.CurrentLanguage : Language.Spanish);
        }

        private void OnDisable()
        {
            LanguageManager.OnLanguageChanged -= RefreshLabels;
        }

        public void ConfigureBin(WasteCategory cat, string titleEs, string titleEn, string subEs, string subEn)
        {
            acceptedCategory = cat;
            titleSpanish = titleEs;
            titleEnglish = titleEn;
            subtitleSpanish = subEs;
            subtitleEnglish = subEn;
            RefreshLabels(LanguageManager.Instance != null ? LanguageManager.Instance.CurrentLanguage : Language.Spanish);
        }

        public void RefreshLabels(Language lang)
        {
            bool isEs = lang == Language.Spanish;
            if (titleText != null)
                titleText.text = isEs ? titleSpanish : titleEnglish;

            if (subtitleText != null)
                subtitleText.text = isEs ? subtitleSpanish : subtitleEnglish;
        }

        public void SetHintHighlight(bool active)
        {
            _isHintActive = active;
            if (hintHighlightRing != null)
                hintHighlightRing.SetActive(active);

            if (!active)
                transform.localScale = _baseScale;
        }

        private void Update()
        {
            if (_isHintActive)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.06f;
                transform.localScale = _baseScale * pulse;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TryProcessCollider(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryProcessCollider(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision != null && collision.collider != null)
                TryProcessCollider(collision.collider);
        }

        private void TryProcessCollider(Collider other)
        {
            if (other == null) return;

            EcoTrashItem item = other.GetComponentInParent<EcoTrashItem>();
            if (item != null && !item.IsSorted)
            {
                if (EcoLevelManager.Instance != null)
                {
                    EcoLevelManager.Instance.OnItemDroppedInBin(item, this);
                }
            }
        }

        #region Desktop / Pointer Click-to-Drop Support

        public void OnPointerClick(PointerEventData eventData)
        {
            TryDropSelectedItemHere();
        }

        private void OnMouseDown()
        {
            TryDropSelectedItemHere();
        }

        private void TryDropSelectedItemHere()
        {
            if (EcoTrashItem.CurrentlySelectedItem != null && !EcoTrashItem.CurrentlySelectedItem.IsSorted)
            {
                if (EcoLevelManager.Instance != null)
                {
                    EcoLevelManager.Instance.OnItemDroppedInBin(EcoTrashItem.CurrentlySelectedItem, this);
                }
            }
        }

        #endregion
    }
}
