Shader "THRenderer/THRiver"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _ColorDensity("Color Density", range(0, 1)) = 0.2
        _DeepColor ("Deep Color", Color) = (0.11, 0.24, 0.44, 1)
        _DeepLerp ("Deep Density", float) = 1
        _CellDensity ("Cell Density", float) = 1
        _CellPower ("Cell power", float) = 1
        _Normal("Normal Texture", 2D) = "Bump"{}
        _NormalStrength("Normal Strength", range(0.995, 1.005)) = 1
        _WaterFresnel ("Fresnel Effect", range(0, 1)) = 0.5
        _ShadowMap("Shadow map", 2D) = "white" {}

        _FlashingThreshold ("Flashing Threshold", range(0, 1)) = 0.2
        _FlashingPower ("Flashing Power", float) = 5
        _FlashingDensity ("Flashing Density", float) = 1

        _FoamDeep ("Foam Deep", float) = 0.1

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", float) = 10.0
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", float) = 1
        [Enum(off, 0, On, 1)] _ZWrite("Z Write", float) = 1
     }

     SubShader
     {
        Pass
        {
            Name "THRiverShader"
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
            #pragma fragment RiverFragment
            ENDHLSL
        }
    }
}
