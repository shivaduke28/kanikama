Shader "Kanikama/Additive"
{
    Properties
    {
        _Tex1("Tex1", 2D) = "black" {}
        _Tex2("Tex2", 2D) = "black" {}
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

            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag

            sampler2D _Tex1;
            sampler2D _Tex2;


            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                float2 uv = IN.localTexcoord.xy;
                float4 color = tex2D(_Tex1, uv);
                color += tex2D(_Tex2, uv);
                color.a = 1;
                return color;
            }
            ENDCG
        }
    }
}
