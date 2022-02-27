Shader "Kanikama/Composite/Unlit"
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
            #include "UnityCG.cginc"
            #include "./CGIncludes/KanikamaComposite.hlsl"

            #pragma target 3.5
            #pragma require 2darray

            #pragma vertex vert
            #pragma fragment frag

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
                o.uv = v.uv;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 color;
                color.rgb = KanikamaSampleLightmapArray(i.uv);
                color.a = 1;
                return color;
            }
            ENDCG
        }
    }
}