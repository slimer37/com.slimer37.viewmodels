Shader "Hidden/HDRP/WriteNormal"
{
    Properties
    {
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch switch2

    // #pragma enable_d3d11_debug_symbols

    //enable GPU instancing support
    #pragma multi_compile_instancing
    #pragma multi_compile _ DOTS_INSTANCING_ON

    // List all the attributes needed in your shader (will be passed to the vertex shader)
    // you can see the complete list of these attributes in VaryingMesh.hlsl
    #define ATTRIBUTES_NEED_TEXCOORD0
    #define ATTRIBUTES_NEED_NORMAL
    #define ATTRIBUTES_NEED_TANGENT

    // List all the varyings needed in your fragment shader
    #define VARYINGS_NEED_TEXCOORD0
    #define VARYINGS_NEED_TANGENT_TO_WORLD

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassRenderers.hlsl"

    #pragma vertex Vert
    #pragma fragment Frag

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }

        Pass
        {
            Name "NormalToColorPass"
            Tags { "LightMode" = "NormalToColorPass" }

            Blend Off
            ZWrite On
            ZTest LEqual

            Cull Back

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/VertMesh.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"

            PackedVaryingsType Vert(AttributesMesh inputMesh)
            {
                VaryingsType varyingsType;
                varyingsType.vmesh = VertMesh(inputMesh);
                return PackVaryingsType(varyingsType);
            }

            void Frag(PackedVaryingsType packedInput, out float4 outNormalBuffer : SV_Target0)
            {
                FragInputs input = UnpackVaryingsMeshToFragInputs(packedInput.vmesh);

                NormalData normalData;
                normalData.normalWS = input.tangentToWorld[2].xyz;
                normalData.perceptualRoughness = 0.5;
                EncodeIntoNormalBuffer(normalData, outNormalBuffer);
            }

            ENDHLSL
        }
    }
}
