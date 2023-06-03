Shader "Kanikama/KanikamaSurface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [NoScaleOffset] _MetallicSmoothnessMap("Metallic Smoothness Map", 2D) = "white" {}
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1.0
        [NoScaleOffset] _OcclusionMap("Occlusion Map", 2D) = "white" {}
        [NoScaleOffset] _ParallaxMap("Parallax Map", 2D) = "white" {}
        _Parallax ("Parallax", Range(0, 0.1)) = 0.02
        [Header(Kanikama)]
        [KeywordEnum(Array, Directional)] _Kanikama_Mode("Kanikama Mode", Float) = 0
        [Toggle(_KANIKAMA_DIRECTIONAL_SPECULAR)] _Kanikama_Directional_Specular("Kanikama Directional Specular", Float) = 0
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
        #pragma shader_feature_local_fragment _ _KANIKAMA_MODE_DIRECTIONAL
        #pragma shader_feature_local_fragment _ _KANIKAMA_DIRECTIONAL_SPECULAR

        #pragma target 3.0

        #include "UnityPBSLighting.cginc"
        #include "./Kanikama.hlsl"

        sampler2D _MainTex;
        sampler2D _MetallicSmoothnessMap;
        sampler2D _BumpMap;
        half _BumpScale;
        sampler2D _OcclusionMap;
        sampler2D _ParallaxMap;

        half _Glossiness;
        half _Metallic;
        half _Parallax;
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
            half roughness = SmoothnessToRoughness(s.Smoothness);
            half3 diffuse;
            half3 specular;
            KanikamaSample(data.lightmapUV, s.Normal, data.worldViewDir, roughness, diffuse, specular);
            gi.indirect.diffuse += diffuse * s.Occlusion;
            gi.indirect.specular += specular * s.Occlusion;
        }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.lightmapUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;
            uv += ParallaxOffset(tex2D(_ParallaxMap, uv).r, _Parallax, IN.viewDir);
            half4 base = tex2D(_MainTex, uv) * _Color;
            o.Albedo = base.rgb;
            o.Alpha = base.a;
            half2 ms = tex2D(_MetallicSmoothnessMap, uv).xw;
            o.Metallic = _Metallic * ms.x;
            o.Smoothness = _Glossiness * ms.y;
            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, uv), _BumpScale);
            o.Occlusion = tex2D(_OcclusionMap, uv).r;
        }
        ENDCG
    }
    FallBack "Diffuse"
}