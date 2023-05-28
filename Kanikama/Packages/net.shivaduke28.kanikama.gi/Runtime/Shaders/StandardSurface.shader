Shader "Kanikama/StandardSurface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1.0
        [KeywordEnum(Array, Directional)] _Kanikama_GI_Mode("KanikamaGI Mode", Float) = 0
        [PerRendererData]_Udon_LightmapArray("LightmapArray", 2DArray) = ""{}
        [PerRendererData]_Udon_LightmapIndArray("LightmapIndArray", 2DArray) = ""{}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "KanikamaGI"="True"
        }
        LOD 200

        CGPROGRAM
        #pragma surface surf Kanikama fullforwardshadows vertex:vert addshadow
        #pragma shader_feature_local_fragment _ _KANIKAMA_GI_MODE_DIRECTIONAL

        #pragma target 3.0

        #include "UnityPBSLighting.cginc"
        #include "./KanikamaGI.hlsl"

        sampler2D _MainTex;
        sampler2D _BumpMap;
        half _BumpScale;

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;


        struct Input
        {
            float2 uv_MainTex;
            float2 lightmapUV;
            float3 worldPos;
            float3 viewDir;
            float3 worldNormal;
            INTERNAL_DATA
        };

        inline half4 LightingKanikama(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
        {
            return LightingStandard(s, viewDir, gi);
        }

        inline void LightingKanikama_GI(SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi)
        {
            Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal,
                                                                        lerp(unity_ColorSpaceDielectricSpec.rgb,
                                                                             s.Albedo, s.Metallic));
            gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
            gi.indirect.diffuse += KanikamaGISample(data.lightmapUV, s.Normal);
        }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.lightmapUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;
            #if defined(_PARALLAX)
                uv += ParallaxOffset(tex2D(_ParallaxMap, uv).r, _Parallax, IN.viewDir);
            #endif
            half4 base = tex2D(_MainTex, uv) * _Color;
            o.Albedo = base.rgb;
            o.Alpha = base.a;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, uv), _BumpScale);
            o.Occlusion = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}