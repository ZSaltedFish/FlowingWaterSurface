#ifndef TH_RIVER_UV_CONTROL
#define TH_RIVER_UV_CONTROL

// 旋转UV（弧度）
float2 RotateUV(float2 uv, float angle)
{
    float cos = cos(angle);
    float sin = sin(angle);

    float x = cos * uv.x - sin * uv.y;
    float y = sin * uv.x + cos * uv.y;
    return float2(x, y);
}

float2 RotateUVDirection(float2 uv, float2 direction)
{
    float2 dire = normalize(direction);
    float x = uv.x * dire.x - uv.y * dire.y;
    float y = uv.x * dire.y + uv.y * dire.x;
    return float2(x, y);
}
#endif