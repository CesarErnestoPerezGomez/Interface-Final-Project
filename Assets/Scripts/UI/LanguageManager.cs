using System;
using System.Collections;
using UnityEngine;

namespace EcoSort.UI
{
    public static class VRViewHelper
    {
        public static void StabilizeAndCenterVRView(MonoBehaviour host)
        {
            if (host == null) return;
            ApplySnapshot(true);
            host.StartCoroutine(StabilizeRoutine());
        }

        private static IEnumerator StabilizeRoutine()
        {
            float[] delays = { 0.05f, 0.15f, 0.35f, 0.70f, 1.25f };
            for (int i = 0; i < delays.Length; i++)
            {
                yield return new WaitForSeconds(delays[i]);
                ApplySnapshot(i <= 3);
            }
        }

        public static void ApplySnapshot(bool alignYawAndXZ)
        {
            Camera cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (cam == null) return;

            // Ensure every World-Space Canvas has worldCamera assigned for XR Ray / GraphicRaycaster
            Canvas[] canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] != null && canvases[i].renderMode == RenderMode.WorldSpace && canvases[i].worldCamera == null)
                {
                    canvases[i].worldCamera = cam;
                }
            }

            Transform rigRoot = cam.transform.root;
            if (rigRoot == null) return;

            // Prevent CharacterController / Gravity from pulling the XR Origin downward
            CharacterController cc = rigRoot.GetComponentInChildren<CharacterController>(true);
            if (cc != null && cc.enabled)
            {
                cc.enabled = false;
            }

            Transform[] allChildren = rigRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (allChildren[i] != null && allChildren[i].name == "Gravity" && allChildren[i].gameObject.activeSelf)
                {
                    allChildren[i].gameObject.SetActive(false);
                }
            }

            if (alignYawAndXZ)
            {
                Vector3 flatFwd = cam.transform.forward;
                flatFwd.y = 0f;
                if (flatFwd.sqrMagnitude > 0.001f)
                {
                    float angleDiff = Vector3.SignedAngle(flatFwd.normalized, Vector3.forward, Vector3.up);
                    if (Mathf.Abs(angleDiff) > 10f)
                    {
                        rigRoot.RotateAround(cam.transform.position, Vector3.up, angleDiff);
                    }
                }

                Vector2 camXZ = new Vector2(cam.transform.position.x, cam.transform.position.z);
                if (camXZ.magnitude > 0.12f)
                {
                    rigRoot.position -= new Vector3(cam.transform.position.x, 0f, cam.transform.position.z);
                }
            }

            // Keep eye height comfortable (~1.36m) in front of the sorting table and UI cards
            float targetEyeY = 1.36f;
            float currentEyeY = cam.transform.position.y;
            if (Mathf.Abs(currentEyeY - targetEyeY) > 0.12f)
            {
                float deltaY = targetEyeY - currentEyeY;
                rigRoot.position += new Vector3(0f, deltaY, 0f);
            }
        }
    }

    public enum Language
    {
        Spanish = 0,
        English = 1
    }

    public class LanguageManager : MonoBehaviour
    {
        private static LanguageManager _instance;
        private const string PrefsKey = "EcoSort_Language";

        public static LanguageManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<LanguageManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("LanguageManager");
                        _instance = go.AddComponent<LanguageManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [SerializeField]
        private Language _currentLanguage = Language.Spanish;

        public static event Action<Language> OnLanguageChanged;

        public Language CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    PlayerPrefs.SetInt(PrefsKey, (int)_currentLanguage);
                    PlayerPrefs.Save();
                    OnLanguageChanged?.Invoke(_currentLanguage);
                }
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                
                int saved = PlayerPrefs.GetInt(PrefsKey, 0);
                _currentLanguage = (Language)saved;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void ToggleLanguage()
        {
            CurrentLanguage = _currentLanguage == Language.Spanish ? Language.English : Language.Spanish;
        }

        public void SetLanguage(Language language)
        {
            CurrentLanguage = language;
        }
    }
}
