// NOTE: This shader does NOTE include all the features of the Standard shader.
// Adding all the features of the Standard shader will make the compilation time of this surface shader very long,
// so some of the features have been removed.
Shader "Kanikama/KanikamaStandardSurface"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0

        // [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        // [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        // [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        // _DetailMask("Detail Mask", 2D) = "white" {}
        // _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        // _DetailNormalMapScale("Scale", Float) = 1.0
        // [Normal] _DetailNormalMap("Normal Map", 2D) = "bump" {}
        // [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0

        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0

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
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]

        CGPROGRAM
        #pragma shader_feature_local_fragment _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
        #pragma shader_feature_local_fragment _EMISSION

        // NOTE: need to reduce shader keywords to compile surface shader.
        // #pragma shader_feature_local _METALLICGLOSSMAP
        // #pragma shader_feature_local _DETAIL_MULX2
        // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        // #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
        // #pragma shader_feature_local _GLOSSYREFLECTIONS_OFF

        #define _METALLICGLOSSMAP 1
        #define _PARALLAXMAP 1
        #define _NORMALMAP 1

        #pragma surface surf Kanikama fullforwardshadows vertex:vert addshadow
        #pragma shader_feature_local_fragment _ _KANIKAMA_MODE_DIRECTIONAL
        #pragma shader_feature_local_fragment _ _KANIKAMA_DIRECTIONAL_SPECULAR


        #pragma multi_compile_fog
        #pragma multi_compile_instancing

        #pragma target 3.0

        #include <UnityPBSLighting.cginc>
        #include "./Kanikama.hlsl"

        half4 _Color;
        half _Cutoff;

        sampler2D _MainTex;

        sampler2D _BumpMap;
        half _BumpScale;

        // sampler2D _DetailMask;
        // sampler2D _DetailNormalMap;
        // half _DetailNormalMapScale;

        // sampler2D _SpecGlossMap;
        sampler2D _MetallicGlossMap;
        half _Metallic;
        float _Glossiness;
        float _GlossMapScale;

        sampler2D _OcclusionMap;
        half _OcclusionStrength;

        sampler2D _ParallaxMap;
        half _Parallax;
        // half _UVSec;

        half4 _EmissionColor;
        sampler2D _EmissionMap;

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
            KanikamaGI(data.lightmapUV, s.Normal, data.worldViewDir, roughness, s.Occlusion, diffuse, specular);
            gi.indirect.diffuse += diffuse;
            gi.indirect.specular += specular;
        }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.lightmapUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;
            uv += ParallaxOffset(tex2D(_ParallaxMap, uv).g, _Parallax, IN.viewDir);
            half4 base = tex2D(_MainTex, uv) * _Color;
            o.Albedo = base.rgb;
            o.Alpha = base.a;
            half2 ms = tex2D(_MetallicGlossMap, uv).xw;
            o.Metallic = _Metallic * ms.x;
            o.Smoothness = _Glossiness * ms.y;
            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, uv), _BumpScale);
            o.Occlusion = tex2D(_OcclusionMap, uv).r;
            #ifdef _EMISSION
            o.Emission = tex2D(_EmissionMap, uv) * _EmissionColor;
            #endif
        }
        ENDCG
    }
    FallBack "Diffuse"
    CustomEditor "Kanikama.Editor.Application.KanikamaStandardGUI"
}