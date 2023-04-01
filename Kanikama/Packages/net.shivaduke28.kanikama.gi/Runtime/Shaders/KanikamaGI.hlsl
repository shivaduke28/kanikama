#ifndef KANIKAMA_GI_INCLUDED
#define KANIKAMA_GI_INCLUDED

#include <UnityCG.cginc>
#include <HLSLSupport.cginc>

UNITY_DECLARE_TEX2DARRAY(_Udon_LightmapArray);
static const int KANIKAMA_MAX_LIGHTMAP_COUNT = 64;
half3 _Udon_LightmapColors[KANIKAMA_MAX_LIGHTMAP_COUNT];
int _Udon_LightmapCount;

#if defined(_KANIKAMA_GI_MODE_DIRECTIONAL)

UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_Udon_LightmapIndArray);

inline half3 KanikamaGISampleDirectionalLightmapArray(float2 lightmapUV, float3 normalWorld)
{
    half3 col = 0;
    for (int i = 0; i < _Udon_LightmapCount; i++)
    {
        half3 bakedColor = DecodeLightmap(
            UNITY_SAMPLE_TEX2DARRAY(_Udon_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i))
        ) * _Udon_LightmapColors[i];
        fixed4 bakedDirTex = UNITY_SAMPLE_TEX2DARRAY_SAMPLER(_Udon_LightmapIndArray,
                                                             _Udon_LightmapArray,
                                                             float3(lightmapUV.x, lightmapUV.y, i));
        col += DecodeDirectionalLightmap(bakedColor, bakedDirTex, normalWorld);
    }

    return col;
}

#else // _KANIKAMA_GI_MODE_DIRECTIONAL

inline half3 KanikamaGISampleLightmapArray(float2 lightmapUV)
{
    half3 col = 0;
    for (int i = 0; i < _Udon_LightmapCount; i++)
    {
        col += DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY(
                _Udon_LightmapArray,
                float3(lightmapUV.x, lightmapUV.y, i))
        ) * _Udon_LightmapColors[i];
    }
    return col;
}

#endif // _KANIKAMA_GI_MODE_DIRECTIONAL

half3 KanikamaGISample(float2 lightmapUV, float3 normalWorld)
{
    #if defined(_KANIKAMA_GI_MODE_DIRECTIONAL)
    return KanikamaGISampleDirectionalLightmapArray(lightmapUV, normalWorld);
    #else
    return KanikamaGISampleLightmapArray(lightmapUV);
    #endif
}

#endif // _KANIKAMA_GI_MODE_DIRECTIONAL
