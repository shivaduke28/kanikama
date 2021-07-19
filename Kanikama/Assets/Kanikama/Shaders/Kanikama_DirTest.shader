Shader "Kanikama/DirTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Specular("Specular", Range(0, 1)) = 0.4
        _DiffuseMin("DiffuseMin", Range(0, 1)) = 0.2
        _Threshold("Threshold" ,Range(0,1)) = 0
        _Specular2("Specular2", Range(0, 10)) = 0.4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #define DIRLIGHTMAP_COMBINED

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float3 normal : NORMAL;
                float3 view : TEXCOORD2;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normalWorld : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Specular;
            float _Specular2;

            float _DiffuseMin;
            float _Threshold;
            SamplerState samplerunity_LightmapInd;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = v.uv2 * unity_LightmapST.xy + unity_LightmapST.zw;
                o.normalWorld = UnityObjectToWorldNormal(v.normal);
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                col.rgb = 0;
                half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv2);
                half3 bakedColor = DecodeLightmap(bakedColorTex);
                #ifdef DIRLIGHTMAP_COMBINED
                    fixed4 bakedDirTex = UNITY_SAMPLE_TEX2D(unity_LightmapInd,i.uv2);
                    float3 normalWorld = i.normalWorld;
                    col.rgb += DecodeDirectionalLightmap (bakedColor, bakedDirTex, normalWorld);
                #endif

                float3 bakedDir = bakedDirTex.xyz - 0.5;            
                float diretionality = length(bakedDir);
                float3 lightDir = bakedDir / diretionality;
                float3 spec = dot(lightDir, i.viewDir);
                spec = smoothstep(_Threshold,_Threshold * 1.1, spec) * spec * diretionality * _Specular2;
                col.rgb = bakedColor * (lerp(dot(bakedDir, normalWorld), spec,  _Specular) + _DiffuseMin)  / max(1e-4h, bakedDirTex.w) ;
                return col;
            }

            
            inline half3 DecodeDirectionalLightmap2 (half3 color, fixed4 dirTex, half3 normalWorld)
            {
                // In directional (non-specular) mode Enlighten bakes dominant light direction
                // in a way, that using it for half Lambert and then dividing by a "rebalancing coefficient"
                // gives a result close to plain diffuse response lightmaps, but normalmapped.

                // Note that dir is not unit length on purpose. Its length is "directionality", like
                // for the directional specular lightmaps.

                half halfLambert = dot(normalWorld, dirTex.xyz - 0.5) + 0.5;

                return color * halfLambert / max(1e-4h, dirTex.w);
            }


            ENDCG
        }
    }
}
