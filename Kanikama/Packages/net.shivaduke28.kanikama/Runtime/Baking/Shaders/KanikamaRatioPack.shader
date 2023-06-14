Shader "Kanikama/RatioPack"
{
    Properties
    {
        [NoScaleOffset] _Numerator0("Numerator R", 2D) = "black" {}
        [NoScaleOffset] _Numerator1("Numerator G", 2D) = "black" {}
        [NoScaleOffset] _Numerator2("Numerator B", 2D) = "black" {}
        [NoScaleOffset] _Numerator3("Numerator A", 2D) = "black" {}
        [NoScaleOffset] _Denominator0("Denominator R", 2D) = "white" {}
        [NoScaleOffset] _Denominator1("Denominator G", 2D) = "white" {}
        [NoScaleOffset] _Denominator2("Denominator B", 2D) = "white" {}
        [NoScaleOffset] _Denominator3("Denominator A", 2D) = "white" {}
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _Numerator0;
            sampler2D _Numerator1;
            sampler2D _Numerator2;
            sampler2D _Numerator3;
            sampler2D _Denominator0;
            sampler2D _Denominator1;
            sampler2D _Denominator2;
            sampler2D _Denominator3;


            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 numerator = float4(
                    Luminance(tex2D(_Numerator0, uv).rgb),
                    Luminance(tex2D(_Numerator1, uv).rgb),
                    Luminance(tex2D(_Numerator2, uv).rgb),
                    Luminance(tex2D(_Numerator3, uv).rgb));
                float4 denominator = float4(
                    Luminance(tex2D(_Denominator0, uv).rgb),
                    Luminance(tex2D(_Denominator1, uv).rgb),
                    Luminance(tex2D(_Denominator2, uv).rgb),
                    Luminance(tex2D(_Denominator3, uv).rgb));
                float4 col;
                col.r = denominator.r == 0 ? 0 : numerator.r / max(1e-4, denominator.r);
                col.g = denominator.g == 0 ? 0 : numerator.g / max(1e-4, denominator.g);
                col.b = denominator.b == 0 ? 0 : numerator.b / max(1e-4, denominator.b);
                col.a = denominator.a == 0 ? 0 : numerator.a / max(1e-4, denominator.a);
                return col;
            }
            ENDCG
        }
    }
}