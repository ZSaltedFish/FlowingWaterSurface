#ifndef TH_RIVER_FORWARD_INCLUDED
#define TH_RIVER_FORWARD_INCLUDED

#include "THRiverIODefine.hlsl"
TEXTURE2D(_Normal);
SAMPLER(sampler_Normal);
TEXTURE2D(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);
TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);
TEXTURE2D(_ShadowMap);
SAMPLER(sampler_ShadowMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
UNITY_DEFINE_INSTANCED_PROP(float4, _Normal_ST)
UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_DEFINE_INSTANCED_PROP(float4, _DeepColor)
UNITY_DEFINE_INSTANCED_PROP(float, _DeepLerp)
UNITY_DEFINE_INSTANCED_PROP(float, _CellDensity)
UNITY_DEFINE_INSTANCED_PROP(float, _CellPower)
UNITY_DEFINE_INSTANCED_PROP(float, _NormalStrength)
UNITY_DEFINE_INSTANCED_PROP(float, _WaterFresnel)
UNITY_DEFINE_INSTANCED_PROP(float, _ColorDensity)
UNITY_DEFINE_INSTANCED_PROP(float, _FlashingThreshold)
UNITY_DEFINE_INSTANCED_PROP(float, _FlashingPower)
UNITY_DEFINE_INSTANCED_PROP(float, _FlashingDensity)
UNITY_DEFINE_INSTANCED_PROP(float, _FoamDeep)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

RiverVaryings RiverVertex(RiverAttributes input)
{
    RiverVaryings output = (RiverVaryings) 0;
    
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    float3 positionWS = TransformObjectToWorld(input.positionOS);
    float4 positionCS = TransformWorldToHClip(positionWS);
    output.positionCS = positionCS;
    output.positionWS = float4(positionWS, 1);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.tangentWS = float4(TransformObjectToWorldNormal(input.tangentOS.xyz) * input.tangentOS.w, 1);
    output.viewDirWS = GetWorldSpaceViewDir(positionWS);
    output.color = input.color;

    return output;
}

#include "THRiverSetup.hlsl"
#include "THRiverNoise.hlsl"
#include "THRiverSceneFunction.hlsl"
#include "THRiverFlowing.hlsl"

float4 RiverFragment(RiverVaryings input) : SV_Target
{
    River river;
    UNITY_SETUP_INSTANCE_ID(river);
    SetupRiver(input, river);
    float3 color = RiverRender(river, input);
    return float4(color, 1);

}

#endif