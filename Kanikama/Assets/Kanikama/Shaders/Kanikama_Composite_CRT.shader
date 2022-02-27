Shader "Kanikama/Composite/CRT"
{
    Properties
    {
        [NoScaleOffset] knkm_LightmapArray("knkm_LightmapArray", 2DArray) = "" {}
        knkm_Count("knkm_Count", int) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {

            CGPROGRAM
            #define _KANIKAMA_MODE_ARRAY
            #include "UnityCustomRenderTexture.cginc"
            #include "./CGIncludes/KanikamaComposite.hlsl"

            #pragma target 3.5
            #pragma require 2darray

            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                float2 uv = IN.localTexcoord.xy;
                float4 color;
                color.rgb = KanikamaSampleLightmapArray(uv);
                color.a = 1;
                return color;
            }
            ENDCG
        }
    }
}
