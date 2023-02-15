Shader "Kanikama/Subtract"
{
    Properties
    {
        _Tex0("Texture 0", 2D) = "white" {}
        _Tex1("Texture 1", 2D) = "black" {}
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
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _Tex0;
            float4 _Tex0_ST;
            sampler2D _Tex1;
            float4 _Tex1_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv0 = TRANSFORM_TEX(v.uv, _Tex0);
                o.uv1 = TRANSFORM_TEX(v.uv, _Tex1);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col;
                col.rgb = max(0, tex2D(_Tex0, i.uv0).rgb - tex2D(_Tex1, i.uv1).rgb);
                col.a = 1;
                return col;
            }
            ENDCG
        }
    }
}