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
// Array, Directional, or Directional Specular

#if defined(SHADER_API_D3D11) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))

static const int KNKM_MAX_COUNT = 100;
half3 knkm_Colors[KNKM_MAX_COUNT];
int knkm_Count;
UNITY_DECLARE_TEX2DARRAY(knkm_LightmapArray);

// Array
#if defined(_KANIKAMA_MODE_ARRAY)

inline half3 KanikamaSampleLightmapArray(float2 lightmapUV)
{
    half3 col = 0;
#if defined(_KANIKAMA_PACK)
    const int loopCount = ceil( (float)knkm_Count / 3.0);
    for(int i = 0; i < loopCount; i++)
    {
        float3 c = DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY(knkm_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i)));
        col += c.r * knkm_Colors[i * 3];
        col += c.g * knkm_Colors[i * 3 + 1];
        col += c.b * knkm_Colors[i * 3 + 2];
    }
#else
    for (int i = 0; i < knkm_Count; i++)
    {
        col += DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY(knkm_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i))) * knkm_Colors[i];
    }
#endif

    return col;
}

#else
// Directional or Directional Specular

UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(knkm_LightmapIndArray);

// Directional
#if defined(_KANIKAMA_MODE_DIRECTIONAL)

inline half3 KanikamaSampleDirectionalLightmapArray(float2 lightmapUV, float3 normalWorld)
{
    half3 col = 0;
#if _KANIKAMA_PACK
    const int loopCount = ceil((float)knkm_Count * 0.3333333);
    for(int i = 0; i < loopCount; i++)
    {
        float3 c = DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY(knkm_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i)));
        int i3 = i * 3;
        for(int j = 0; j < 3 ; j++)
        {
            int k = i3 + j;
            if (k >= knkm_Count)
            {
                break;
            }
            col += DecodeDirectionalLightmap(c[j] * knkm_Colors[k], UNITY_SAMPLE_TEX2DARRAY_SAMPLER(knkm_LightmapIndArray, knkm_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i3 )), normalWorld);;
        }
    }
#else
    for (int i = 0; i < knkm_Count; i++)
    {
        half3 bakedColor = DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY(knkm_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i))) * knkm_Colors[i];
        fixed4 bakedDirTex = UNITY_SAMPLE_TEX2DARRAY_SAMPLER(knkm_LightmapIndArray, knkm_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i));
        col += DecodeDirectionalLightmap(bakedColor, bakedDirTex, normalWorld);
    }
#endif

    return col;
}

#else // Directional Specular

#include "UnityStandardBRDF.cginc"

// Directional lightmap specular based on BakeryDirectionalLightmapSpecular in Bakery.cginc by Mr F
// https://geom.io/bakery/wiki/
inline void KanikamaDirectionalLightmapSpecular(float2 lightmapUV, half3 normalWorld, half3 viewDir, half roughness,
                                                out half3 diffuse, out half3 specular)
{
    #if _KANIKAMA_PACK
    const int loopCount = ceil((float)knkm_Count * 0.3333333);
    for(int i = 0; i < loopCount; i++)
    {
        float3 c = DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY(knkm_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i)));
        int i3 = i * 3;
        for(int j = 0; j < 3 ; j++)
        {
            int k = i3 + j;
            if (k >= knkm_Count)
            {
                break;
            }
            half3 bakedColor = c[j] * knkm_Colors[k];
            half4 dirTex = UNITY_SAMPLE_TEX2DARRAY_SAMPLER(knkm_LightmapIndArray, knkm_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i3));
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
    #else
    for (int i = 0; i < knkm_Count; i++)
    {
        half3 bakedColor = DecodeLightmap(
            UNITY_SAMPLE_TEX2DARRAY(knkm_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i))) * knkm_Colors[i];
        half4 dirTex = UNITY_SAMPLE_TEX2DARRAY_SAMPLER(knkm_LightmapIndArray, knkm_LightmapArray,
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
    #endif
}
#endif // defined(_KANIKAMA_MODE_DIRECTIONAL)
#endif // defined(_KANIKAMA_MODE_ARRAY)
#endif // defined(SHADER_API_D3D11)...
#endif // defined(_KANIKAMA_MODE_SINGLE)
#endif // KANIKAMA_COMPOSITE_INCLUDED
