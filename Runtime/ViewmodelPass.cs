using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[assembly: InternalsVisibleTo("Slimer37.Viewmodel.Rendering.Editor")]

namespace Slimer37.Viewmodel.Rendering
{
    [Serializable]
    internal class ViewmodelPass : CustomPass
    {
        [SerializeField] private Camera _overlayViewmodelCamera;
        [SerializeField] private float _fieldOfView;
        [SerializeField] private LayerMask _viewmodelMask;
        [SerializeField] private bool _prepass;

        protected override bool executeInSceneView => false;

        private static Material _normalMaterial;

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            _normalMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/HDRP/WriteNormal"));
        }

        protected override void Cleanup()
        {
            CoreUtils.Destroy(_normalMaterial);
        }

        protected override void AggregateCullingParameters(ref ScriptableCullingParameters cullingParameters,
            HDCamera hdCamera)
            => cullingParameters.cullingMask |= (uint)_viewmodelMask.value;

        protected override void Execute(CustomPassContext ctx)
        {
            if (_overlayViewmodelCamera == null || !ctx.hdCamera.camera.CompareTag("MainCamera")) return;

            _overlayViewmodelCamera.CopyFrom(ctx.hdCamera.camera);
            _overlayViewmodelCamera.fieldOfView = _fieldOfView;
            _overlayViewmodelCamera.cullingMask = _viewmodelMask;

            var depthState = new RenderStateBlock(RenderStateMask.Depth)
            {
                depthState = new DepthState(true, CompareFunction.LessEqual)
            };

            if (_prepass)
            {
                CustomPassUtils.RenderFromCamera(ctx, _overlayViewmodelCamera, ctx.cameraNormalBuffer,
                    ctx.cameraDepthBuffer, ClearFlag.None, _viewmodelMask, RenderQueueType.All, _normalMaterial, overrideRenderState: depthState);
            }
            else
            {

                CustomPassUtils.RenderFromCamera(ctx, _overlayViewmodelCamera, ctx.cameraColorBuffer,
                    ctx.cameraDepthBuffer, ClearFlag.Depth, _viewmodelMask, overrideRenderState: depthState);
            }
        }
    }
}