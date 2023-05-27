#ifndef KANIKAMA_LIGHTMAP_UV_INCLUDED
#define KANIKAMA_LIGHTMAP_UV_INCLUDED

void LightmapScaleAndOffset_float(out float2 scale, out float2 offset)
{
    scale = unity_LightmapST.xy;
    offset = unity_LightmapST.zw;
}

void LightmapUV_float(float2 uv1, out float2 lightmapUV)
{
    lightmapUV = uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
}
#endif
