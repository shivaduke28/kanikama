#ifndef KANIKAMA_GI_INCLUDED
#define KANIKAMA_GI_INCLUDED

#include <UnityCG.cginc>
#include <HLSLSupport.cginc>

UNITY_DECLARE_TEX2DARRAY(_Udon_LightmapArray);
static const int KANIKAMA_MAX_LIGHTMAP_COUNT = 64;
half3 _Udon_LightmapColors[KANIKAMA_MAX_LIGHTMAP_COUNT];
int _Udon_LightmapCount;

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

#if defined(_KANIKAMA_GI_MODE_DIRECTIONAL) || defined(_KANIKAMA_GI_MODE_DIRECTIONAL_SPECULAR)

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

#if defined(_KANIKAMA_GI_MODE_DIRECTIONAL_SPECULAR)

#include "UnityStandardBRDF.cginc"

// Directional lightmap specular based on BakeryDirectionalLightmapSpecular in Bakery.cginc by Mr F
// https://geom.io/bakery/wiki/
inline void KanikamaDirectionalLightmapSpecular(float2 lightmapUV, half3 normalWorld, half3 viewDir, half roughness,
                                                out half3 diffuse, out half3 specular)
{
    for (int i = 0; i < _Udon_LightmapCount; i++)
    {
        half3 bakedColor = DecodeLightmap(
            UNITY_SAMPLE_TEX2DARRAY(_Udon_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i))) * _Udon_LightmapColors[i];
        half4 dirTex = UNITY_SAMPLE_TEX2DARRAY_SAMPLER(_Udon_LightmapIndArray, _Udon_LightmapArray,
                                                       float3(lightmapUV.x, lightmapUV.y, i));
        half3 dominantDir = dirTex.xyz - 0.5;
        half3 halfDir = Unity_SafeNormalize(normalize(dominantDir) + viewDir);
        half nh = saturate(dot(normalWorld, halfDir));
        half spec = GGXTerm(nh, roughness);
        half halfLambert = dot(normalWorld, dominantDir) + 0.5;
        half3 diff = bakedColor * halfLambert / max(1e-4h, dirTex.w);
        diffuse += diff;
        specular += spec * bakedColor;
    }
}

#endif

#endif // _KANIKAMA_GI_MODE_DIRECTIONAL


half3 KanikamaGISample(float2 lightmapUV)
{
    return KanikamaGISampleLightmapArray(lightmapUV);
}

half3 KanikamaGISample(float2 lightmapUV, float3 normalWorld)
{
    #if defined(_KANIKAMA_GI_MODE_DIRECTIONAL)
    return KanikamaGISampleDirectionalLightmapArray(lightmapUV, normalWorld);
    #else
    return KanikamaGISampleLightmapArray(lightmapUV);
    #endif
}

void KanikamaGISample(float2 lightmapUV, half3 normalWorld, half3 viewDir, half roughness,
                      out half3 diffuse, out half3 specular)
{
    #if defined(_KANIKAMA_GI_MODE_DIRECTIONAL_SPECULAR)
    KanikamaDirectionalLightmapSpecular(lightmapUV, normalWorld, viewDir, roughness, diffuse, specular);
    #else
    diffuse = KanikamaGISample(lightmapUV, normalWorld);
    specular = 0;
    #endif
}

#endif // KANIKAMA_GI_INCLUDED
