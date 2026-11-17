using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using EcoSort.UI;

namespace EcoSort.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class EcoTrashItem : MonoBehaviour, IPointerClickHandler
    {
        [Header("Trash Item Info")]
        [SerializeField] private WasteCategory category = WasteCategory.GreenOrganic;
        [SerializeField] private string itemNameSpanish = "Cáscara de plátano";
        [SerializeField] private string itemNameEnglish = "Banana peel";
        [SerializeField] private TextMeshProUGUI floatingLabelText;

        private Rigidbody _rb;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private Vector3 _initialScale;
        private bool _isSorted = false;
        private bool _isDraggingMouse = false;
        private float _mouseDragDepth = 1.2f;
        private Camera _mainCam;

        public static EcoTrashItem CurrentlySelectedItem { get; private set; }

        public WasteCategory Category => category;
        public string ItemNameSpanish => itemNameSpanish;
        public string ItemNameEnglish => itemNameEnglish;
        public bool IsSorted => _isSorted;

        public string GetLocalizedName()
        {
            bool isEs = LanguageManager.Instance == null || LanguageManager.Instance.CurrentLanguage == Language.Spanish;
            return isEs ? itemNameSpanish : itemNameEnglish;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _initialScale = transform.localScale;
        }

        private void OnEnable()
        {
            LanguageManager.OnLanguageChanged += RefreshLabel;
            RefreshLabel(LanguageManager.Instance != null ? LanguageManager.Instance.CurrentLanguage : Language.Spanish);
        }

        private void OnDisable()
        {
            LanguageManager.OnLanguageChanged -= RefreshLabel;
            if (CurrentlySelectedItem == this)
                CurrentlySelectedItem = null;
        }

        public void ConfigureItem(WasteCategory cat, string nameEs, string nameEn)
        {
            category = cat;
            itemNameSpanish = nameEs;
            itemNameEnglish = nameEn;
            RefreshLabel(LanguageManager.Instance != null ? LanguageManager.Instance.CurrentLanguage : Language.Spanish);
        }

        public void RecordSpawnPose()
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _initialScale = transform.localScale;
        }

        private void RefreshLabel(Language lang)
        {
            if (floatingLabelText != null)
            {
                floatingLabelText.text = lang == Language.Spanish ? itemNameSpanish : itemNameEnglish;
            }
        }

        private void Update()
        {
            if (_isSorted) return;

            // Safety respawn if dropped off the table onto the floor
            if (!_isDraggingMouse && transform.position.y < 0.22f)
            {
                ResetToSpawn();
            }

            // Gentle float pulse when selected via click
            if (CurrentlySelectedItem == this && !_isDraggingMouse)
            {
                float bob = Mathf.Sin(Time.time * 5f) * 0.04f;
                transform.position = _initialPosition + new Vector3(0f, 0.14f + bob, 0f);
            }
        }

        public void ResetToSpawn()
        {
            _isSorted = false;
            _isDraggingMouse = false;
            if (CurrentlySelectedItem == this)
                CurrentlySelectedItem = null;

            gameObject.SetActive(true);
            transform.localScale = _initialScale;
            transform.SetPositionAndRotation(_initialPosition, _initialRotation);

            if (_rb != null)
            {
                _rb.isKinematic = false;
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
        }

        public void MarkAsSortedAndAbsorb(Vector3 targetBinPos)
        {
            if (_isSorted) return;
            _isSorted = true;

            if (CurrentlySelectedItem == this)
                CurrentlySelectedItem = null;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }

            StartCoroutine(AnimateIntoBin(targetBinPos));
        }

        private IEnumerator AnimateIntoBin(Vector3 targetPos)
        {
            Vector3 startPos = transform.position;
            Vector3 startScale = transform.localScale;
            float duration = 0.35f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Arc trajectory into bin opening
                Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
                pos.y += Mathf.Sin(t * Mathf.PI) * 0.18f;
                transform.position = pos;
                transform.localScale = Vector3.Lerp(startScale, startScale * 0.25f, t);
                yield return null;
            }

            gameObject.SetActive(false);
        }

        #region Desktop / Editor Mouse & Pointer Support

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isSorted) return;
            SelectThisItem();
        }

        private void OnMouseDown()
        {
            if (_isSorted) return;
            _mainCam = Camera.main;
            if (_mainCam == null) return;

            SelectThisItem();
            _isDraggingMouse = true;
            _mouseDragDepth = Vector3.Distance(_mainCam.transform.position, transform.position);

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }
        }

        private void OnMouseDrag()
        {
            if (!_isDraggingMouse || _isSorted || _mainCam == null) return;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = _mouseDragDepth;
            Vector3 worldPos = _mainCam.ScreenToWorldPoint(mousePos);
            worldPos.y = Mathf.Max(0.55f, worldPos.y);
            transform.position = Vector3.Lerp(transform.position, worldPos, Time.deltaTime * 18f);
        }

        private void OnMouseUp()
        {
            if (!_isDraggingMouse) return;
            _isDraggingMouse = false;

            if (!_isSorted && _rb != null)
            {
                _rb.isKinematic = false;
            }
        }

        public void SelectThisItem()
        {
            if (CurrentlySelectedItem != null && CurrentlySelectedItem != this && !CurrentlySelectedItem._isSorted)
            {
                CurrentlySelectedItem.ResetToSpawn();
            }

            CurrentlySelectedItem = this;
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }

            if (EcoLevelManager.Instance != null)
            {
                EcoLevelManager.Instance.OnItemSelected(this);
            }
        }

        #endregion
    }
}
