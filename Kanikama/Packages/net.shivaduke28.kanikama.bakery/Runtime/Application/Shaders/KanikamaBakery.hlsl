#ifndef KANIKAMA_BAKERY_INCLUDED
#define KANIKAMA_BAKERY_INCLUDED

#include <UnityCG.cginc>
#include <HLSLSupport.cginc>
#include "Packages/net.shivaduke28.kanikama/Runtime/Application/Shaders/Kanikama.hlsl"

#if defined(_KANIKAMA_MODE_BAKERY_MONOSH)

UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_Udon_LightmapIndArray);

#if defined(_KANIKAMA_DIRECTIONAL_SPECULAR)
#include "UnityStandardBRDF.cginc"
#endif


// MonoSH based on BakeryDirectionalLightmapSpecular in Bakery.cginc by Mr F
// https://geom.io/bakery/wiki/
inline void KanikamaSampleBakeryMonoSH(float2 lightmapUV, half3 normalWorld, half3 viewDir, half roughness,
                                       out half3 diffuse, out half3 specular)
{
    diffuse = 0;
    specular = 0;
    for (int i = 0; i < _Udon_LightmapCount; i++)
    {
        float3 dominantDir = UNITY_SAMPLE_TEX2DARRAY_SAMPLER(_Udon_LightmapIndArray, _Udon_LightmapArray, float3(lightmapUV, i)).xyz;
        float3 L0 = DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY(_Udon_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i)))
            * _Udon_LightmapColors[i];

        float3 nL1 = dominantDir * 2 - 1;
        float3 L1x = nL1.x * L0 * 2;
        float3 L1y = nL1.y * L0 * 2;
        float3 L1z = nL1.z * L0 * 2;

        // MAY: support BAKERY_SHNONLINEAR
        float3 sh = L0 + normalWorld.x * L1x + normalWorld.y * L1y + normalWorld.z * L1z;

        diffuse += max(sh, 0.0);

        #if defined(_KANIKAMA_DIRECTIONAL_SPECULAR)
        dominantDir = nL1;
        // float focus = saturate(length(dominantDir));
        // roughness *= focus;
        half3 halfDir = Unity_SafeNormalize(normalize(dominantDir) + viewDir);
        half nh = saturate(dot(normalWorld, halfDir));
        half spec = GGXTerm(nh, roughness);

        specular += max(spec * sh, 0.0);
        #endif
    }
}

#endif // _KANIKAMA_MODE_BAKERY_MONOSH

void KanikamaBakerySample(float2 lightmapUV, half3 normalWorld, half3 viewDir, half roughness,
                    out half3 diffuse, out half3 specular)
{
    #if defined(_KANIKAMA_MODE_DIRECTIONAL)
    KanikamaSampleDirectional(lightmapUV, normalWorld, viewDir, roughness, diffuse, specular);
    #elif defined (_KANIKAMA_MODE_BAKERY_MONOSH)
    KanikamaSampleBakeryMonoSH(lightmapUV, normalWorld, viewDir, roughness, diffuse, specular);
    #else
    diffuse = KanikamaGISampleLightmapArray(lightmapUV);
    specular = 0;
    #endif
}

#endif // KANIKAMA_BAKERY_INCLUDED
