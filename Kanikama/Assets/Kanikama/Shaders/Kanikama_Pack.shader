Shader "Kanikama/Pack"
{
    Properties
    {
        _Tex0("Texture 0", 2D) = "white" {}
        _Tex1("Texture 1", 2D) = "white" {}
        _Tex2("Texture 2", 2D) = "white" {}
        _Tex3("Texture 3", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _Tex0;
            sampler2D _Tex1;
            sampler2D _Tex2;
            sampler2D _Tex3;

            half4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                half4 col;
                col.r = Luminance(tex2D(_Tex0, uv).rgb);
                col.g = Luminance(tex2D(_Tex1, uv).rgb);
                col.b = Luminance(tex2D(_Tex2, uv).rgb);
                col.a = Luminance(tex2D(_Tex3, uv).rgb);
                return col;
            }
            ENDCG
        }
    }
}
