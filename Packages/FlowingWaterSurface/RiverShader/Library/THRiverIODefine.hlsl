#ifndef TH_RIVER_IO_DEFINE_INCLUDED
#define TH_RIVER_IO_DEFINE_INCLUDED

#include "THRiverCommon.hlsl"
struct RiverAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 color : COLOR0;
    float4 tangentOS : TANGENT;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct RiverVaryings
{
    float4 positionWS : TEXCOORD0;
    float3 normalWS : TEXCOORD1;
    float4 tangentWS : TEXCOORD2;
    float3 viewDirWS : TEXCOORD3;
    float4 positionCS : SV_POSITION;
    float4 screenPos : TEXCOORD4;
    float4 color : TEXCOORD5;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct WaterFlashing
{
    float flashingThreshold;
    float flashingPower;
    float flashingDensity;
};

struct WaterFoam
{
    float foamDeep;
};

struct River
{
    float4 position;
    float3 normal;
    float4 tangent;
    float3 biTangent;
    float3 flowingDire;
    
    float3 viewDire;
    float4 screenPos;
    float flowSpeed;
    float roughness;
    float3 F0;
    float waterFresnel;
    float3 deepColor;
    float deepValue;
    float3 color;
    float colorDensity;
    
    WaterFlashing waterFlashing;
    WaterFoam waterFoam;
};
#endif