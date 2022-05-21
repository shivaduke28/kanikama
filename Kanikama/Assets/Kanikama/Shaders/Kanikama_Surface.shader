Shader "Kanikama/Surface"
{
    Properties
    {
        [KeywordEnum(None, Single, Array, Directional, Directional_Specular)] _Kanikama_Mode("Kanikama Mode", Float) = 0
        [Toggle(_KANIKAMA_PACK)] _Kanikama_Pack("Enable RGB Packing", Float) = 0

        [Space]
        [Header(Base)]
        [Space]
        _MainTex("Albedo", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)

        [Space]
        [Header(Metallic and Smoothness)]
        [Space]
        [NoScaleOffset] _MetallicGlossMap("Metallic (R) & Smoothness (A)", 2D) = "white" {}
        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5

        [Space]
        [Header(Bump)]
        [Space]
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1.0
        [Toggle(_PARALLAX)] _ParallaxEnable("Enable Parallax", Float) = 0
        [NoScaleOffset] _ParallaxMap("Height Map", 2D) = "black" {}
        _Parallax("Height Scale", Range(0.005, 0.08)) = 0.02

        [Space]
        [Header(Occlusion)]
        [Space]
        [NoScaleOffset] _OcclusionMap("Occlusion", 2D) = "white" {}
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0

        [Space]
        [Header(Emission)]
        [Space]
        [Toggle(_EMISSION)] _Emission("Enable Emission", Float) = 0
        [NoScaleOffset] _EmissionMap("Emission Map", 2D) = "white" {}
        [HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0)
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        LOD 300

        CGPROGRAM
        #include "UnityStandardUtils.cginc"
        #include "UnityPBSLighting.cginc"
        #include "./CGIncludes/KanikamaComposite.hlsl"
        #include "./Standard/KanikamaGlobalIllumination.hlsl"

        #pragma surface surf Kanikama fullforwardshadows vertex:vert addshadow
        #pragma target 3.0

        #pragma shader_feature_local_fragment _ _KANIKAMA_MODE_SINGLE _KANIKAMA_MODE_ARRAY _KANIKAMA_MODE_DIRECTIONAL _KANIKAMA_MODE_DIRECTIONAL_SPECULAR
        #pragma shader_feature_local_fragment _ _KANIKAMA_PACK

        #pragma shader_feature_local_fragment _ _EMISSION
        #pragma shader_feature_local_fragment _ _PARALLAX

        fixed4 _Color;
        sampler2D _MainTex;
        sampler2D _BumpMap;
        half _BumpScale;
        sampler2D _ParallaxMap;
        half _Parallax;
        sampler2D _MetallicGlossMap;
        half _Metallic;
        half _Glossiness;
        sampler2D _OcclusionMap;
        half _OcclusionStrength;

        #if defined(_EMISSION)
            sampler2D _EmissionMap;
            half3 _EmissionColor;
        #endif
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
            gi = KanikamaGlobalIllumination(data, s.Occlusion, s.Normal, g);
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
            half2 mg = tex2D(_MetallicGlossMap, uv).ra;
            o.Metallic = mg.r * _Metallic;
            o.Smoothness = mg.g * _Glossiness;
            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, uv), _BumpScale);
            o.Occlusion = LerpOneTo(tex2D(_OcclusionMap, uv).g, _OcclusionStrength);

            #if defined(_EMISSION)
                o.Emission = tex2D(_EmissionMap, uv).rgb * _EmissionColor;
            #endif
        }
        ENDCG
    }
    FallBack "Diffuse"
}