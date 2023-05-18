#ifndef TH_RIVER_SETUP_INCLUDED
#define TH_RIVER_SETUP_INCLUDED

void SetupRiver(RiverVaryings input, inout River river)
{
    river.position = input.positionWS;
    river.normal = input.normalWS;
    river.tangent = input.tangentWS;
    river.flowingDire = river.tangent.xyz;
    river.viewDire = input.viewDirWS;
    river.screenPos = input.screenPos;
    river.UV = input.UV;
    river.flowSpeed = input.color.r;
    river.roughness = 0.04;
    river.F0 = float3(1, 1, 1);
    river.biTangent.xyz = -normalize(cross(river.normal, river.tangent));
    river.waterFresnel = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _WaterFresnel);
    river.deepColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _DeepColor);
    river.deepValue = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _DeepLerp);
    river.riverLength = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _RiverLength);
    river.color = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Color);
    river.colorDensity = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _ColorDensity);

    river.waterFlashing = (WaterFlashing) 0;
    river.waterFlashing.flashingThreshold = 1 - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FlashingThreshold);
    river.waterFlashing.flashingPower = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FlashingPower);
    river.waterFlashing.flashingDensity = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FlashingDensity);

    river.waterFoam.foamDeep = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FoamDeep);
    
    river.waterWave.waveScale = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _WaveScale);
    river.waterWave.waveStepValue = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _WaveStepValue);

    river.waterFlowing.OffsetUV = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _OffsetUV);
    river.waterFlowing.MapTile = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _NormalTile).xy;
    river.waterFlowing.StepValue = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _NormalStepValue);

}

#endif