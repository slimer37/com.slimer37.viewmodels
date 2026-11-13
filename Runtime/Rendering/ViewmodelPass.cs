using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.RendererUtils;

namespace Slimer37.Viewmodels.Rendering
{
    [Serializable]
    internal class ViewmodelPass : CustomPass
    {
        public ViewmodelLayerRenderer Source { get; set; }

        protected override bool executeInSceneView => false;

        private Camera _overlayViewmodelCamera;

        private ShaderTagId[] _forwardTags;

        private static readonly int s_AmbientOcclusionTexture = Shader.PropertyToID("_AmbientOcclusionTexture");

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            var cameraObject = new GameObject("Viewmodel Overlay Camera")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            cameraObject.transform.SetParent(Source.TargetCamera.transform);

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

            if (_overlayViewmodelCamera == null || camera != Source.TargetCamera) return;

            _overlayViewmodelCamera.CopyFrom(camera);

            _overlayViewmodelCamera.fieldOfView = Source.FieldOfView;
            _overlayViewmodelCamera.nearClipPlane = Source.NearClipPlane;
            _overlayViewmodelCamera.farClipPlane = Mathf.Max(Source.FarClipPlane, Source.NearClipPlane + 0.001f);
            _overlayViewmodelCamera.cullingMask = Source.ViewmodelMask;

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

            ctx.cmd.SetGlobalTexture(s_AmbientOcclusionTexture, TextureXR.GetBlackTexture());

            using (new CustomPassUtils.OverrideCameraRendering(ctx, _overlayViewmodelCamera))
            {
                CoreUtils.DrawRendererList(ctx.cmd, ctx.renderContext.CreateRendererList(desc));
            }
        }
    }
}