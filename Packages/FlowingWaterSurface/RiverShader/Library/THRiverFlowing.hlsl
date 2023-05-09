#ifndef TH_RIVER_FLOWING_INCLUDED
#define TH_RIVER_FLOWING_INCLUDED

#define PI 3.141592653

float ShadowMapping(float v)
{
    return SAMPLE_TEXTURE2D(_ShadowMap, sampler_ShadowMap, float2(v, 0.5)).r;
}

float RiverDFunction(float NdotH, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH2 = NdotH * NdotH;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = denom * denom * PI;
    return a2 / denom;
}

float RiverGSubFunction(float NdotW, float k)
{
    return NdotW / lerp(NdotW, 1.0, k);
}

float RiverGFunction(float NdotL, float NdotV, float roughness)
{
    float k = (1.0 + roughness) * (1.0 + roughness) / 8;
    return RiverGSubFunction(NdotL, k) * RiverGSubFunction(NdotV, k);
}

float RiverFLightFunction(float HdotL, float3 F0)
{
    float fresnel = exp2((-5.55473 * HdotL - 6.98316) * HdotL);
    return lerp(fresnel, 1.0, F0);
}

float3 DirectionLightFunction(float roughness, float3 F0, float3 mainLightColor, float NdotH, float NdotL, float NdotV, float HdotL)
{
    float D = RiverDFunction(NdotH, roughness);
    float G = RiverGFunction(NdotL, NdotV, roughness);
    float F = RiverFLightFunction(HdotL, F0);
    float3 section = D * G * F / (4 * NdotL * NdotV);
    return section * mainLightColor * NdotL * PI;
}

float3 DiffuseWater(float3 lightColor, float NdotL)
{
    return ShadowMapping(NdotL) * lightColor;
}

float2 LUT_Approx(float roughness, float NoV)
{
    // [ Lazarov 2013, "Getting More Physical in Call of Duty: Black Ops II" ]
    // Adaptation to fit our G term.
    const float4 c0 = { -1, -0.0275, -0.572, 0.022 };
    const float4 c1 = { 1, 0.0425, 1.04, -0.04 };
    float4 r = roughness * c0 + c1;
    float a004 = min(r.x * r.x, exp2(-9.28 * NoV)) * r.x + r.y;
    float2 AB = float2(-1.04, 1.04) * a004 + r.zw;
    return saturate(AB);
}

float3 RiverFIndireLightFunction(float NdotV, float roughness, float3 F0)
{
    float fresnel = exp2((-5.55473 * NdotV - 6.98316) * NdotV);
    return F0 + fresnel * saturate(1 - roughness - F0);
}

float3 IndireSpecFunction(float3 envColor, float roughness, float NdotV, float occlusion, float3 F0)
{
    float2 LUT = LUT_Approx(roughness, NdotV);
    float3 indireLight = RiverFIndireLightFunction(NdotV, roughness, F0);
    float3 factor = envColor * (indireLight * LUT.r + LUT.g);
    return factor * occlusion;
}

float3 IndireDiffFunction(float NdotV, float3 N, float metallic, float roughness, float occlusion, float3 F0, float3 ambient)
{
    float3 KS = RiverFIndireLightFunction(NdotV, roughness, F0);
    float3 KD = (1 - KS) * (1 - metallic);
    return ambient * KD * occlusion;
}

float3 ReflectEEnvironment(float roughness, float3 normalWS)
{
    float3 environment = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, normalWS, 0.0).rgb;
    environment /= roughness * roughness + 1.0;
    return environment;
}

float3 FlowingRiverLight(River river, Light light, float3 N, float3 V, float3 NdotV, float deep)
{
    float3 L = normalize(light.direction);
    float3 H = normalize(L + V);
    float HdotN = max(dot(H, N), 1e-5);
    float HdotL = max(dot(H, L), 1e-5);
    float NdotL = max(dot(N, L), 1e-5);

    float shadow = light.shadowAttenuation * light.distanceAttenuation;
    /*float3 direLight = DirectionLightFunction(river.roughness, river.F0, light.color, HdotN,
                        NdotL, NdotV, HdotL) * shadow*/;
    return VoronoiTest(river, light, HdotN) * shadow + GetFoam(river, light, deep);
    //return direLight;
}

float3 UnderwaterColor(River river, float2 sceneUV, float deep)
{
    float3 sceneColor = GetSceneColor(sceneUV);
    
    float depthValue = max(0, river.deepValue);
    float p = 5;
    float min = max(0, depthValue - p);
    float max = depthValue + p;
    
    float lerpValue = smoothstep(min, max, deep);
    
    return lerp(sceneColor, river.deepColor, lerpValue);

}

float3 RiverRender(River river, RiverVaryings input)
{
    float3 N = GetNormal(river);
    float3 V = normalize(river.viewDire);
    float3 reflectView = -reflect(V, N);
    
    float2 sceneUV = GetSceneUV(input.positionCS) + N.xz * 0.05;
    
    float deep = GetDeepFromDepthTexture(sceneUV, river.position);
    float3 sceneColor = UnderwaterColor(river, sceneUV, deep);
    
    float NdotV = max(dot(N, V), 1e-5);
    float3 envColor = ReflectEEnvironment(river.roughness, reflectView);
    
    float th = 1 - NdotV;
    th = th * th * th * th * th;
    float3 ambient = lerp(envColor, sceneColor, 1 - th);
    ambient = lerp(ambient, river.color, river.colorDensity);
    
    float3 indireSpecLight = IndireSpecFunction(ambient, river.roughness, NdotV, 1.0, river.F0);
    
    float4 shadowCoord = TransformWorldToShadowCoord(river.position);
    Light mainLight = GetMainLight(shadowCoord);
    
    float3 direLight = FlowingRiverLight(river, mainLight, N, V, NdotV, deep);
    uint additionalLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < additionalLightCount; ++i)
    {
        Light additionalLight = GetAdditionalLight(i, river.position);
        direLight += FlowingRiverLight(river, additionalLight, N, V, NdotV, deep);
    }
    
    float3 uvSet = UVSet(river.position.xz, 1, 5, -10, 0.7).xxx;
    return direLight + indireSpecLight + uvSet;
}
#endif