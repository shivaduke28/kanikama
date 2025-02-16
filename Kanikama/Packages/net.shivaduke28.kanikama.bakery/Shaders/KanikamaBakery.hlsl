﻿// Based on Bakery.cginc by Mr F
// https://geom.io/bakery/wiki/
#ifndef KANIKAMA_BAKERY_INCLUDED
#define KANIKAMA_BAKERY_INCLUDED

#include <UnityCG.cginc>
#include <HLSLSupport.cginc>
#include <UnityStandardBRDF.cginc>
#include "Packages/net.shivaduke28.kanikama/Shaders/Kanikama.hlsl"

#if defined(_KANIKAMA_MODE_BAKERY_MONOSH) && defined(BAKERY_MONOSH)

UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_Udon_LightmapIndArray);

#if defined(_KANIKAMA_DIRECTIONAL_SPECULAR)
#include "UnityStandardBRDF.cginc"
#endif

float KanikamaSHEvaluateDiffuseL1Geomerics(float L0, float3 L1, float3 n)
{
    // average energy
    float R0 = L0;

    // avg direction of incoming light
    float3 R1 = 0.5f * L1;

    // directional brightness
    float lenR1 = length(R1);

    // linear angle between normal and direction 0-1
    //float q = 0.5f * (1.0f + dot(R1 / lenR1, n));
    //float q = dot(R1 / lenR1, n) * 0.5 + 0.5;
    float q = dot(normalize(R1), n) * 0.5 + 0.5;

    // power for q
    // lerps from 1 (linear) to 3 (cubic) based on directionality
    float p = 1.0f + 2.0f * lenR1 / R0;

    // dynamic range constant
    // should vary between 4 (highly directional) and 0 (ambient)
    float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);

    return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
}

inline void KanikamaBakeryMonoSH(float3 lightTex, float3 dirTex, half3 normalWorld, half3 viewDir, half roughness,
                                       inout half3 diffuse, inout half3 specular)
{
    float3 L0 = lightTex;
    float3 dominantDir = dirTex;
    float3 nL1 = dominantDir * 2 - 1;
    float3 L1x = nL1.x * L0 * 2;
    float3 L1y = nL1.y * L0 * 2;
    float3 L1z = nL1.z * L0 * 2;

    float3 sh;
    #if _KANIKAMA_BAKERY_SHNONLINEAR
    //sh.r = shEvaluateDiffuseL1Geomerics(L0.r, float3(L1x.r, L1y.r, L1z.r), normalWorld);
    //sh.g = shEvaluateDiffuseL1Geomerics(L0.g, float3(L1x.g, L1y.g, L1z.g), normalWorld);
    //sh.b = shEvaluateDiffuseL1Geomerics(L0.b, float3(L1x.b, L1y.b, L1z.b), normalWorld);

    float lumaL0 = dot(L0, 1);
    float lumaL1x = dot(L1x, 1);
    float lumaL1y = dot(L1y, 1);
    float lumaL1z = dot(L1z, 1);
    float lumaSH = KanikamaSHEvaluateDiffuseL1Geomerics(lumaL0, float3(lumaL1x, lumaL1y, lumaL1z), normalWorld);

    sh = L0 + normalWorld.x * L1x + normalWorld.y * L1y + normalWorld.z * L1z;
    float regularLumaSH = dot(sh, 1);
    //sh *= regularLumaSH < 0.001 ? 1 : (lumaSH / regularLumaSH);
    sh *= lerp(1, lumaSH / regularLumaSH, saturate(regularLumaSH*16));

    #else
    sh = L0 + normalWorld.x * L1x + normalWorld.y * L1y + normalWorld.z * L1z;
    #endif

    diffuse += max(sh, 0.0);

    #if defined(_KANIKAMA_DIRECTIONAL_SPECULAR)
    dominantDir = nL1;
    // float focus = saturate(length(dominantDir));
    // roughness *= focus;
    half3 halfDir = Unity_SafeNormalize(normalize(dominantDir) + viewDir);
    half nh = saturate(dot(normalWorld, halfDir));
    half spec = GGXTerm(nh, roughness);
    sh = L0 + dominantDir.x * L1x + dominantDir.y * L1y + dominantDir.z * L1z;

    specular += max(spec * sh, 0.0);
    #endif
}

inline void KanikamaSampleBakeryMonoSH(float2 lightmapUV, half3 normalWorld, half3 viewDir, half roughness,
                                       out half3 diffuse, out half3 specular)
{
    diffuse = 0;
    specular = 0;
    for (int i = 0; i < _Udon_LightmapCount; i++)
    {
        float3 dirTex = UNITY_SAMPLE_TEX2DARRAY_SAMPLER(_Udon_LightmapIndArray, _Udon_LightmapArray, float3(lightmapUV, i)).xyz;
        float3 lightTex = DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY(_Udon_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i)))
            * _Udon_LightmapColors[i];

        half3 diff = 0;
        half3 spec = 0;
        KanikamaBakeryMonoSH(lightTex, dirTex, normalWorld, viewDir, roughness, diff, spec);
        diffuse += diff;
        specular += spec;
    }
}

#endif // _KANIKAMA_MODE_BAKERY_MONOSH

void KanikamaBakeryGI(float2 lightmapUV, half3 normalWorld, half3 viewDir, half smoothness,
    half occlusion, out half3 diffuse, out half3 specular)
{
    half roughness = SmoothnessToRoughness(smoothness);
    #if defined(_KANIKAMA_MODE_DIRECTIONAL)
    KanikamaSampleDirectional(lightmapUV, normalWorld, viewDir, roughness, diffuse, specular);
    #elif defined (_KANIKAMA_MODE_BAKERY_MONOSH) && defined(BAKERY_MONOSH)
    KanikamaSampleBakeryMonoSH(lightmapUV, normalWorld, viewDir, roughness, diffuse, specular);
    #else
    diffuse = KanikamaGISampleLightmapArray(lightmapUV);
    specular = 0;
    #endif
    diffuse *= occlusion;
    specular *= occlusion;
}

#endif // KANIKAMA_BAKERY_INCLUDED
