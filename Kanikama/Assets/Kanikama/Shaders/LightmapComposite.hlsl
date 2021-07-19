#define SHADER_API_D3D11

#include "UnityCG.cginc"
#include "UnityCustomRenderTexture.cginc"

#pragma target 3.5
#pragma require 2darray

static const int MAX_COUNT = 10;

float4 _Colors[MAX_COUNT];
float _Intensities[MAX_COUNT];
int _TexCount;
float _Max;


UNITY_DECLARE_TEX2DARRAY(_Tex2DArray);

//float4 frag(v2f_customrendertexture IN) : COLOR
//{
//    float2 uv = IN.localTexcoord.xy;
//    float3 color = float3(0, 0, 0);

//    for (int i = 0; i < _TexCount; i++)
//    {
//        color += DecodeLightmapRGBM(UNITY_SAMPLE_TEX2DARRAY(_Tex2DArray, float3(uv.x, uv.y, i)), unity_Lightmap_HDR) * _Colors[i].rgb * _Intensities[i];
//    }
    
//    float4 encode = UnityEncodeRGBM(color, _Max);
//    return encode;
//}

float4 frag(v2f_customrendertexture IN) : COLOR
{
    float2 uv = IN.localTexcoord.xy;
    float4 color = float4(0, 0, 0, 0);

    for (int i = 0; i < _TexCount; i++)
    {
        color += UNITY_SAMPLE_TEX2DARRAY(_Tex2DArray, float3(uv.x, uv.y, i)) * _Colors[i] * _Intensities[i];
    }

    return color;
}