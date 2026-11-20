using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Slimer37.Viewmodels.Rendering
{
    [ExecuteAlways]
    public class ViewmodelLayerRenderer : MonoBehaviour
    {
        [field: SerializeField, Range(1e-5f, 179f)] public float FieldOfView { get; set; } = 90f;
        [field: SerializeField, Min(0.001f)] public float NearClipPlane { get; set; } = 0.01f;
        [field: SerializeField, Min(0.002f)] public float FarClipPlane { get; set; } = 25f;
        [field: SerializeField] public LayerMask ViewmodelMask { get; set; }
        [field: SerializeField] public Camera TargetCamera { get; set; }

        [SerializeField, HideInInspector] private CustomPassVolume _volume;

        private ViewmodelPass _viewmodelPass;

        public static ViewmodelLayerRenderer _instance;

        private void ApplySettings()
        {
            _viewmodelPass.Source = this;

            _volume.targetCamera = TargetCamera;

            _viewmodelPass.enabled = TargetCamera;
        }

        private void RestorePassFromVolume()
        {
            if (!_volume) return;

            _viewmodelPass = (ViewmodelPass)_volume.customPasses.Find(p => p is ViewmodelPass);
            ApplySettings();
        }

        private void Reset()
        {
            ViewmodelMask = LayerMask.GetMask("UI");
            TargetCamera = Camera.main;

            // Prevent volume assignment if found existing component
            if (TryGetComponent(out ViewmodelLayerRenderer _)) return;

            _volume ??= GetComponent<CustomPassVolume>();

            RestorePassFromVolume();
        }

        private void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && FindObjectsByType<ViewmodelLayerRenderer>().Length > 1)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    UnityEditor.EditorUtility.DisplayDialog("Cannot add",
                        $"Can't add more than one {nameof(ViewmodelLayerRenderer)} to the scene.", "OK");

                    DestroyImmediate(this);
                };

                return;
            }
#endif

            if (_instance && _instance != this)
            {
                Debug.LogWarning($"Only 1 {nameof(ViewmodelLayerRenderer)} allowed in scene.");
                return;
            }

            _instance = this;

            if (_volume) return;

            _volume = gameObject.AddComponent<CustomPassVolume>();
            _volume.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;

            _viewmodelPass = _volume.AddPassOfType<ViewmodelPass>();

            _volume.injectionPoint = CustomPassInjectionPoint.BeforePostProcess;

            _viewmodelPass.name = $"Viewmodel Pass ({name})";

            ApplySettings();
        }

        private void OnEnable()
        {
            if (_volume)
            {
                RestorePassFromVolume();

                _volume.enabled = true;
            }
        }

        private void OnDisable()
        {
            if (_volume) _volume.enabled = false;
        }

        private void OnDestroy()
        {
            if (_volume)
            {
                if (Application.isPlaying)
                {
                    Destroy(_volume);
                }
            }

#if UNITY_EDITOR
            if (Application.isPlaying) return;

            var volume = _volume;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (volume) DestroyImmediate(volume);
            };
#endif
        }

        private void OnValidate()
        {
            if (_viewmodelPass == null) return;

            FarClipPlane = Mathf.Max(FarClipPlane, NearClipPlane);

            ApplySettings();
        }
    }
}