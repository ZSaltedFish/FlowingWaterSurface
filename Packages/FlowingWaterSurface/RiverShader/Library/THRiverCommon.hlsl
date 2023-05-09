#ifndef TH_RIVER_COMMON
#define TH_RIVER_COMMON

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

float4 Triplanar(texture2D tex, SamplerState ss, float3 positionWS, float3 normalWS, float tile = 1, float blend = 1)
{
    float3 nodeUV = positionWS * tile;
    float3 nodeBlend = pow(abs(normalWS), blend);
    nodeBlend /= dot(nodeBlend, 1.0);
    float4 nodeX = SAMPLE_TEXTURE2D(tex, ss, nodeUV.zy);
    float4 nodeY = SAMPLE_TEXTURE2D(tex, ss, nodeUV.xz);
    float4 nodeZ = SAMPLE_TEXTURE2D(tex, ss, nodeUV.xy);
    float4 blendColor = nodeX * nodeBlend.x + nodeY * nodeBlend.y + nodeZ * nodeBlend.z;
    return blendColor;
}

float3 RiverUnpackNormal(float4 normalTexColor)
{
    return UnpackNormalmapRGorAG(normalTexColor);
}

float3 RiverBlendNormal(float3 v0, float3 v1)
{
    return normalize(float3(v0.xy + v0.xy, v0.z * v1.z));
}

float3 CameraPositionWS()
{
    return _WorldSpaceCameraPos;
}

inline float CameraFarPlane()
{
    return _ProjectionParams.z;
}

inline float CameraNearPlane()
{
    return _ProjectionParams.y;

}

float3 NormalStrength(float3 normal, float strength)
{
    float3 n = float3(normal.rg * strength, lerp(1, normal.b, saturate(strength)));
    return n;
}
#endif