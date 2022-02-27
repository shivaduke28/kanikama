#ifndef KANIKAMA_COMPOSITE_INCLUDED
#define KANIKAMA_COMPOSITE_INCLUDED

#include "UnityCG.cginc"
#include "HLSLSupport.cginc"

#if defined(_KANIKAMA_MODE_SINGLE)
sampler2D knkm_Lightmap;

inline half3 KanikamaSampleLightmap(float2 lightmapUV)
{
    return DecodeLightmap(tex2D(knkm_Lightmap, lightmapUV));
}

#else

#if defined(SHADER_API_D3D11) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))

static const int KNKM_MAX_COUNT = 100;
half3 knkm_Colors[KNKM_MAX_COUNT];
int knkm_Count;
UNITY_DECLARE_TEX2DARRAY(knkm_LightmapArray);

inline half3 KanikamaSampleLightmapArray(float2 lightmapUV)
{
    half3 col = 0;
    for (int i = 0; i < knkm_Count; i++)
    {
        col += DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY(knkm_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i))) * knkm_Colors[i];
    }

    return col;
}

#if defined(_KANIKAMA_MODE_DIRECTIONAL)
UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(knkm_LightmapIndArray);

inline half3 KanikamaSampleDirectionalLightmapArray(float2 lightmapUV, float3 normalWorld)
{
    half3 col = 0;
    for (int i = 0; i < knkm_Count; i++)
    {
        half3 bakedColor = DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY(knkm_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i))) * knkm_Colors[i];
        fixed4 bakedDirTex = UNITY_SAMPLE_TEX2DARRAY_SAMPLER(knkm_LightmapIndArray, knkm_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i));
        col += DecodeDirectionalLightmap(bakedColor, bakedDirTex, normalWorld);
    }

    return col;
}
#endif

#endif
#endif
#endif