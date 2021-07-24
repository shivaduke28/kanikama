Shader "Kanikama/Composite"
{
    Properties
    {
        _Tex2DArray("_Tex2DArray", 2DArray) = "" {}
        _TexCount("_TexCount", int) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {

            CGPROGRAM

            #include "UnityCG.cginc"
            #include "UnityCustomRenderTexture.cginc"

            #pragma target 3.5
            #pragma require 2darray

            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag


            static const int MAX_COUNT = 100;

            float4 _Colors[MAX_COUNT];
            int _TexCount;

            UNITY_DECLARE_TEX2DARRAY(_Tex2DArray);

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                float2 uv = IN.localTexcoord.xy;
                float4 color;

                float3 col = 0;

                for (int i = 0; i < _TexCount; i++)
                {
                    col += DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY(_Tex2DArray, float3(uv.x, uv.y, i)) * _Colors[i]);
                }

                color.rgb = col;
                color.a = 1;

                return color;
            }
            ENDCG
        }
    }
}
