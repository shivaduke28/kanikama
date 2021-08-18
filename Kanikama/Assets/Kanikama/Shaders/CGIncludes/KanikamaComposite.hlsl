#ifndef KANIKAMA_COMPOSITE_INCLUDED
#define KANIKAMA_COMPOSITE_INCLUDED

#include "UnityCG.cginc"
#include "HLSLSupport.cginc"

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
        col += DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY(_LightmapArray, float3(lightmapUV.x, lightmapUV.y, i))) * _LightmapColors[i];
    }

    return col;
}

#endif
#endif