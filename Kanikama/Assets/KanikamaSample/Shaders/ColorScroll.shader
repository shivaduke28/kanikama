Shader "Kanikama/Sample/ColorScroll"
{
    Properties
    {
        _Scale("Scale", Range(0,2)) = 1
        _Speed("Speed", Range(-10,10) ) = 1
        _Partition("Partition", Range(1, 10)) = 2
        _Rotation("Rotation", float) = 0
        _Emission("Emission", Range(0,10)) = 1
        _Random("Random", Range(0,1)) = 1
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

            float _Speed;
            float _Scale;
            float _Partition;
            float _Emission;
            float _Rotation;
            float _Random;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // https://gist.github.com/983/e170a24ae8eba2cd174f
            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.b, c.g, K.w, K.z), float4(c.g, c.b, K.x, K.y), step(c.b, c.g));
                float4 q = lerp(float4(p.x, p.y, p.w, c.r), float4(c.r, p.y, p.z, p.x), step(p.x, c.r));

                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(float3(c.x + K.x, c.x + K.y, c.x + K.z)) * 6.0 - float3(K.w, K.w, K.w));
                return c.z * lerp(float3(K.x, K.x, K.x), clamp(p - float3(K.x, K.x, K.x), 0.0, 1.0), c.y);
            }

            float2 rot(float2 uv, float t)
            {
                return float2(cos(t) * uv.x - sin(t) * uv.y, sin(t) * uv.x + cos(t) * uv.y);
            }

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 col;
                float2 uv = i.uv;
                uv = rot(uv - float2(0.5, 0.5), _Rotation * 3.141582 / 360) + float2(0.5, 0.5);
                float s = _Partition;
                float y = floor(uv.y * s) / s;
                float hue = uv.x * _Scale + y + _Time.y * _Speed;

                col = hsv2rgb(float3(hue, 1, 1));
                col *= _Emission;

                col *= step(_Random, random(float2(_Time.y, 0.5)).x);

                return fixed4(col, 1);
            }



            ENDCG
        }
    }
}
