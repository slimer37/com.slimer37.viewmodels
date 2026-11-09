using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.RendererUtils;
using Object = UnityEngine.Object;

namespace Slimer37.Viewmodel.Rendering
{
    [Serializable]
    public class ViewmodelPass : CustomPass
    {
        [field: SerializeField, Range(1e-5f, 179f)] public float FieldOfView { get; set; }
        [field: SerializeField, Min(0.001f)] public float NearClipPlane { get; set; } = 0.01f;
        [field: SerializeField, Min(0.002f)] public float FarClipPlane { get; set; } = 100f;
        [field: SerializeField] public LayerMask ViewmodelMask { get; set; }

        protected override bool executeInSceneView => false;

        private Camera _overlayViewmodelCamera;
        private Camera _mainCamera;

        private ShaderTagId[] _forwardTags;

        private Camera GetCamera()
        {
            foreach (CustomPassVolume customPassVolume in Object.FindObjectsByType<CustomPassVolume>())
            {
                if (customPassVolume.customPasses.Contains(this))
                {
                    return customPassVolume.targetCamera;
                }
            }

            return null;
        }

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            _mainCamera = GetCamera();

            var cameraObject = new GameObject("Viewmodel Overlay Camera")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            cameraObject.transform.SetParent(_mainCamera.transform);

            _overlayViewmodelCamera = cameraObject.AddComponent<Camera>();
            _overlayViewmodelCamera.enabled = false;

            _forwardTags = new[]
            {
                HDShaderPassNames.s_ForwardOnlyName,
                HDShaderPassNames.s_ForwardName,
                HDShaderPassNames.s_SRPDefaultUnlitName
            };
        }

        protected override void Cleanup()
        {
            if (_overlayViewmodelCamera != null)
            {
                CoreUtils.Destroy(_overlayViewmodelCamera.gameObject);
            }

            _overlayViewmodelCamera = null;
        }

        protected override void Execute(CustomPassContext ctx)
        {
            Camera camera = ctx.hdCamera.camera;

            if (_overlayViewmodelCamera == null || camera != _mainCamera) return;

            _overlayViewmodelCamera.CopyFrom(camera);

            _overlayViewmodelCamera.fieldOfView = FieldOfView;
            _overlayViewmodelCamera.nearClipPlane = NearClipPlane;
            _overlayViewmodelCamera.farClipPlane = Mathf.Max(FarClipPlane, NearClipPlane + 0.001f);
            _overlayViewmodelCamera.cullingMask = ViewmodelMask;

            if (!_overlayViewmodelCamera.TryGetCullingParameters(out ScriptableCullingParameters parameters)) return;

            CullingResults cullingResults = ctx.renderContext.Cull(ref parameters);

            CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer, ctx.cameraDepthBuffer, ClearFlag.Depth);

            var desc = new RendererListDesc(_forwardTags, cullingResults, _overlayViewmodelCamera)
            {
                rendererConfiguration = HDUtils.GetRendererConfiguration(
                    ctx.hdCamera.frameSettings.IsEnabled(FrameSettingsField.AdaptiveProbeVolume),
                    ctx.hdCamera.frameSettings.IsEnabled(FrameSettingsField.Shadowmask)),
                renderQueueRange = RenderQueueRange.all,
                sortingCriteria = SortingCriteria.CommonOpaque & ~SortingCriteria.QuantizedFrontToBack,
                stateBlock = new RenderStateBlock(RenderStateMask.Depth)
                {
                    depthState = new DepthState(true, CompareFunction.LessEqual)
                }
            };

            using (new CustomPassUtils.OverrideCameraRendering(ctx, _overlayViewmodelCamera))
            {
                CoreUtils.DrawRendererList(ctx.cmd, ctx.renderContext.CreateRendererList(desc));
            }
        }
    }
}