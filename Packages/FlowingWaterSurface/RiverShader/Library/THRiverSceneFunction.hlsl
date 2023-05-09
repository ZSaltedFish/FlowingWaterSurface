#ifndef TH_RIVER_SCENE_FUNCTION_INCLUDED
#define TH_RIVER_SCENE_FUNCTION_INCLUDED

float2 GetSceneUV(float4 positionCS)
{
    #if UNITY_UV_STARTS_AT_TOP
    float2 pixel = float2(positionCS.x, _ProjectionParams.x < 0 ? _ScaledScreenParams.y - positionCS.y : positionCS.y);
    #else
    float2 pixel = float2(positionCS.x, _ProjectionParams.x > 0 ? _ScaledScreenParams.y - positionCS.y : positionCS.y);
    #endif
    
    float2 uv = 0;
    uv = pixel.xy / _ScaledScreenParams.xy;
    uv.y = 1.0 - uv.y;
    return uv;
}

inline float3 GetSceneColor(float2 uv)
{
    return SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv);
}

inline float2 GetFlowingUV(float3 positionWS)
{
    return positionWS.xx;
}

float3 GetNormal(River river, float2 uv)
{
    float4 normal = SAMPLE_TEXTURE2D(_Normal, sampler_Normal, uv);
    float3 n = RiverUnpackNormal(normal);
    
    float3x3 transposeTangent = transpose(float3x3(river.tangent.xyz, river.biTangent.xyz, river.normal.xyz));
    float3 normalWS = normalize(mul(n, transposeTangent));
    return normalWS;
}

float3 GetNormal(River river)
{
    float cellDensity = max(0.001, UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CellDensity));
    float u = river.position.x / cellDensity - ceil(river.position.x / cellDensity);
    float v = river.position.z / cellDensity - ceil(river.position.z / cellDensity);
    float2 uv = float2(u, v);
    float2 uv1 = river.tangent.xz * _Time.y * 2 / cellDensity + uv;
    float2 uv2 = river.tangent.xz * -_Time.y / cellDensity + uv;
    
    float3 normal1 = GetNormal(river, uv1);
    float3 normal2 = GetNormal(river, uv2);
    
    float normalStrength = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _NormalStrength);
    float3 nor = RiverBlendNormal(normal1, normal2);
    return NormalStrength(nor, normalStrength);
}

inline float SampleDepthTexture(float2 uv)
{
    return SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
}

float GetDeepFromDepthTexture(float2 uv, float3 posWS)
{
    float3 cameraDire = -1 * mul(UNITY_MATRIX_M, transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))[2].xyz);
    float3 cameraPos = CameraPositionWS();
    float3 p = normalize(posWS - cameraPos);
    float far = CameraFarPlane();
    float near = CameraNearPlane();
    float depth = SampleDepthTexture(uv);
    
    float d1 = (depth / near * (far - near) + 1) / far;
    float dist = 1 / d1;
    //float nf = near * far;
    //float dist = nf / (depth * (far - near) + near);
    dist = dist / abs(dot(p, normalize(cameraDire)));
    return dist - distance(posWS, cameraPos);

}
#endif