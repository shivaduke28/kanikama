#ifndef KANIKAMA_GI_URP_INCLUDED
#define KANIKAMA_GI_URP_INCLUDED

#if !SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#endif

TEXTURE2D_ARRAY(_Udon_LightmapArray);
SAMPLER(sampler_Udon_LightmapArray);

#if defined(_KANIKAMA_GI_MODE_DIRECTIONAL)
TEXTURE2D_ARRAY(_Udon_LightmapIndArray);
#endif

static const int KANIKAMA_MAX_LIGHTMAP_COUNT = 64;
half3 _Udon_LightmapColors[KANIKAMA_MAX_LIGHTMAP_COUNT];
int _Udon_LightmapCount;

inline real3 SampleLightmapArray(float2 lightmapUV, int index, half4 decodeInstructions)
{
    real3 illuminance;
    #ifdef UNITY_LIGHTMAP_FULL_HDR
    real4 encoded = SAMPLE_TEXTURE2D_ARRAY(_Udon_LightmapArray, sampler_Udon_LightmapArray, lightmapUV, index);
    illuminance = DecodeLightmap(encoded, decodeInstructions);
    #else
    illuminance = SAMPLE_TEXTURE2D_ARRAY(_Udon_LightmapArray, sampler_Udon_LightmapArray, lightmapUV, index);
    #endif
    return illuminance * _Udon_LightmapColors[index];
}

// Based on "SampleLightmap" function in GlobalIllumination.hlsl
// NOTE: lightmapUV is supposed to be already transformed.
void KanikamaGI_float(float2 lightmapUV, float3 normalWS, out float3 diffuse)
{
    half4 decodeInstructions = half4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h);
    float3 diffuseLighting = 0;

    #if defined(_KANIKAMA_GI_MODE_DIRECTIONAL)
    for (int i = 0; i < _Udon_LightmapCount; i++)
    {
        real4 direction = SAMPLE_TEXTURE2D_ARRAY(_Udon_LightmapIndArray, sampler_Udon_LightmapArray, lightmapUV, i);
        real3 illuminance = SampleLightmapArray(lightmapUV, i, decodeInstructions);
        real halfLambert = dot(normalWS, direction.xyz - 0.5) + 0.5;
        diffuseLighting += illuminance * halfLambert / max(1e-4, direction.w);
    }

    diffuse = diffuseLighting;
    #else
    for (int i = 0; i < _Udon_LightmapCount; i++)
    {
        diffuseLighting += SampleLightmapArray(lightmapUV, i, decodeInstructions);
    }

    diffuse = diffuseLighting;
    #endif
}

#endif
