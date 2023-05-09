#ifndef TH_RIVER_NOISE_INCLUDED
#define TH_RIVER_NOISE_INCLUDED

inline float unity_noise_randomValue(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

inline float unity_noise_interpolate(float a, float b, float t)
{
    return (1.0 - t) * a + (t * b);
}

inline float unity_valueNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = unity_noise_randomValue(c0);
    float r1 = unity_noise_randomValue(c1);
    float r2 = unity_noise_randomValue(c2);
    float r3 = unity_noise_randomValue(c3);

    float bottomOfGrid = unity_noise_interpolate(r0, r1, f.x);
    float topOfGrid = unity_noise_interpolate(r2, r3, f.x);
    float t = unity_noise_interpolate(bottomOfGrid, topOfGrid, f.y);
    return t;
}

float2 unity_gradientNoise_dir(float2 p)
{
    p = p % 289;
    float x = (34 * p.x + 1) * p.x % 289 + p.y;
    x = (34 * x + 1) * x % 289;
    x = frac(x / 41) * 2 - 1;
    return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
}

float unity_gradientNoise(float2 p)
{
    float2 ip = floor(p);
    float2 fp = frac(p);
    float d00 = dot(unity_gradientNoise_dir(ip), fp);
    float d01 = dot(unity_gradientNoise_dir(ip + float2(0, 1)), fp - float2(0, 1));
    float d10 = dot(unity_gradientNoise_dir(ip + float2(1, 0)), fp - float2(1, 0));
    float d11 = dot(unity_gradientNoise_dir(ip + float2(1, 1)), fp - float2(1, 1));
    fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
    return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
}

float SimpleNoise(float2 UV, float Scale)
{
    float t = 0.0;

    float freq = pow(2.0, float(0));
    float amp = pow(0.5, float(3 - 0));
    t += unity_valueNoise(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(1));
    amp = pow(0.5, float(3 - 1));
    t += unity_valueNoise(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    freq = pow(2.0, float(2));
    amp = pow(0.5, float(3 - 2));
    t += unity_valueNoise(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

    return t;
}

inline float2 unity_voronoi_noise_randomVector(float2 UV, float offset)
{
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)) * 46839.32);
    return float2(sin(UV.y * +offset) * 0.5 + 0.5, cos(UV.x * offset) * 0.5 + 0.5);
}

void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
{
    float2 g = floor(UV * CellDensity);
    float2 f = frac(UV * CellDensity);
    float t = 8.0;
    float3 res = float3(8.0, 0.0, 0.0);

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 lattice = float2(x, y);
            float2 offset = unity_voronoi_noise_randomVector(lattice + g, AngleOffset);
            float d = distance(lattice + offset, f);
            if (d < res.x)
            {
                res = float3(d, offset.x, offset.y);
                Out = res.x;
                Cells = res.y;
            }
        }
    }
}

float GradientNoise(float2 UV, float scale)
{
    return unity_gradientNoise(UV * scale) + 0.5;
}

float GetVoronoiOut(float2 UV, float angleOffset, float cellDensity)
{
    float value, cells;
    Unity_Voronoi_float(UV, angleOffset, cellDensity, value, cells);
    return value;
}

float UVSet(float2 UV, float scale, float timeScale0, float timeScale1, float stepValue)
{
    float time0 = _Time.x * timeScale0;
    float time1 = _Time.x * timeScale1;
    
    float2 UV0 = UV + time0.xx;
    float2 UV1 = UV + time1.xx;
    
    float noise0 = GradientNoise(UV0, scale);
    float noise1 = SimpleNoise(UV1, scale);

    float v = (noise0 + noise1) * 0.5;
    return step(stepValue, v);

}

float3 VoronoiTest(River river, Light light, float HdotN)
{
    float time0 = _Time.x * 5;
    float time1 = _Time.x * 10;
    
    float2 uv = river.position.xz;
    float density = max(0, river.waterFlashing.flashingDensity);
    
    float v0 = 1 - saturate(GetVoronoiOut(uv, time0, density));
    float v1 = 1 - saturate(GetVoronoiOut(uv, time1, density));

    float power0 = pow(v0, 20);
    float power1 = pow(v1, 20);

    float avg = (power0 + power1) * 0.5;
    float value = step(0.55, avg);

    float size = step(river.waterFlashing.flashingThreshold, HdotN);
    return light.color * value * river.waterFlashing.flashingPower * size;
}

float GetFoam(River river, Light light, float deep)
{
    float d = 1 - step(river.waterFoam.foamDeep, deep);
    
    return saturate(light.color * d);
}
#endif