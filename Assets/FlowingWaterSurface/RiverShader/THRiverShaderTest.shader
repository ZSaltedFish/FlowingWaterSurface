Shader "THRenderer/THRiverTest"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _Normal("Normal Texture", 2D) = "Bump"{}

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", float) = 10.0
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", float) = 1.0
        [Enum(off, 0, On, 1)] _ZWrite("Z Write", float) = 1.0
    }

    SubShader
    {
        Pass
        {
            Name "THRiverShaderTest"
            Tags {"LightMode" = "UniversalForward"}
            Blend [_SrcBlend] [_DstBlend]
            Cull [_CullMode]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma multi_complie_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #include "Library/THRiverForward.hlsl"
            #pragma vertex RiverVertex
            #pragma fragment RiverTestFragment
            ENDHLSL
        }
    }
}