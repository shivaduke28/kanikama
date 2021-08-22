#ifndef KANIKAMA_COMPOSITE_INCLUDED
#define KANIKAMA_COMPOSITE_INCLUDED

#include "UnityCG.cginc"
#include "HLSLSupport.cginc"

#if defined(_KANIKAMA_MODE_SINGLE)
sampler2D _Lightmap;

inline half3 SampleLightmap(float2 lightmapUV)
{
    return DecodeLightmap(tex2D(_Lightmap, lightmapUV));
}

#else

#if defined(SHADER_API_D3D11) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))

static const int MAX_COUNT = 100;
half4 _LightmapColors[MAX_COUNT];
int _LightmapCount;
UNITY_DECLARE_TEX2DARRAY(_LightmapArray);

inline half3 SampleLightmapArray(float2 lightmapUV)
{
    half3 col = 0;
    for (int i = 0; i < _LightmapCount; i++)
    {
        col += DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY(_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i))) * _LightmapColors[i].rgb;
    }

    return col;
}

#if defined(_KANIKAMA_MODE_DIRECTIONAL)
UNITY_DECLARE_TEX2DARRAY(_DirectionalLightmapArray);

inline half3 SampleDirectionalLightmapArray(float2 lightmapUV, float3 normalWorld)
{
    half3 col = 0;
    for (int i = 0; i < _LightmapCount; i++)
    {
        half3 bakedColor = DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY(_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i))) * _LightmapColors[i].rgb;
        fixed4 bakedDirTex = UNITY_SAMPLE_TEX2DARRAY(_DirectionalLightmapArray, float3(lightmapUV.x, lightmapUV.y, i));
        col += DecodeDirectionalLightmap(bakedColor, bakedDirTex, normalWorld);
    }

    return col;
}
#endif

#endif
#endif
#endif