Shader "Kanikama/Composite"
{
    Properties
    {
        _LightmapArray("_LightmapArray", 2DArray) = "" {}
        _LightmapCount("_LightmapCount", int) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {

            CGPROGRAM

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
                color.rgb = SampleLightmapArray(uv);
                color.a = 1;
                return color;
            }
            ENDCG
        }
    }
}
