#ifndef TH_RIVER_UV_CONTROL
#define TH_RIVER_UV_CONTROL


float2 RotateUVDirection(float2 uv, float2 direction)
{
    float2 dire = direction;
    float x = uv.x * dire.x - uv.y * dire.y;
    float y = uv.x * dire.y + uv.y * dire.x;
    return float2(x, y);
}
#endif