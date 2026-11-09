using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.RendererUtils;
using Object = UnityEngine.Object;

[assembly: InternalsVisibleTo("Slimer37.Viewmodel.Rendering.Editor")]

namespace Slimer37.Viewmodel.Rendering
{
    [Serializable]
    internal class ViewmodelPass : CustomPass
    {
        [SerializeField, Range(1e-5f, 179f)] private float _fieldOfView;
        [SerializeField, Min(0.001f)] private float _nearClipPlane = 0.01f;
        [SerializeField, Min(0.002f)] private float _farClipPlane = 100f;
        [SerializeField] private LayerMask _viewmodelMask;

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

            _overlayViewmodelCamera.fieldOfView = _fieldOfView;
            _overlayViewmodelCamera.nearClipPlane = _nearClipPlane;
            _overlayViewmodelCamera.farClipPlane = Mathf.Max(_farClipPlane, _nearClipPlane + 0.001f);
            _overlayViewmodelCamera.cullingMask = _viewmodelMask;

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