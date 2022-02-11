Shader "Kanikama/Sample/CRTPreview"
{
    Properties
    {
        [NoScaleOffset] knkm_Lightmap("knkm_Lightmap", 2D) = "black"
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            sampler2D knkm_Lightmap;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed3 col = DecodeLightmap(tex2D(knkm_Lightmap, i.uv));
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}
