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

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _BumpScale("Scale", Float) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0

        [Header(Kanikama)]
        [KeywordEnum(NONE, ARRAY, DIRECTIONAL)] _Kanikama_Mode("Kanikama Mode", Float) = 0
        [Toggle(_KANIKAMA_DIRECTIONAL_SPECULAR)] _Kanikama_Directional_Specular("Kanikama Directional Specular", Float) = 0
        [PerRendererData]_Udon_LightmapArray("LightmapArray", 2DArray) = ""{}
        [PerRendererData]_Udon_LightmapIndArray("LightmapIndArray", 2DArray) = ""{}
        [KeywordEnum(None, Specular, Diffuse Specular)] _Kanikama_LTC("Kanikama LTC", Float) = 0
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

        #pragma surface surf Kanikama fullforwardshadows vertex:vert addshadow
        #pragma shader_feature_local_fragment _ _KANIKAMA_MODE_ARRAY _KANIKAMA_MODE_DIRECTIONAL
        #pragma shader_feature_local_fragment _ _KANIKAMA_DIRECTIONAL_SPECULAR
        #pragma shader_feature_local_fragment _ _KANIKAMA_LTC_SPECULAR _KANIKAMA_LTC_DIFFUSE_SPECULAR


        #pragma multi_compile_fog
        #pragma multi_compile_instancing

        #pragma target 3.0

        #include <UnityPBSLighting.cginc>
        #include "Packages/net.shivaduke28.kanikama/Runtime/Application/Shaders/Kanikama.hlsl"
        #include "Packages/net.shivaduke28.kanikama/Runtime/Application/Shaders/KanikamaLTC.hlsl"

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

        inline half3 LTCDiffuseSpecular(Input IN, SurfaceOutputStandard s, float3 worldNormal, float3 worldView)
        {
            half3 specColor;
            half oneMinusReflectivity = 0;
            half3 diffuseColor = DiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, specColor, oneMinusReflectivity);
            half perceptualRoughness = SmoothnessToPerceptualRoughness(s.Smoothness);
            half3 ltcResult;
            KanikamaLTC(IN.worldPos, worldNormal, worldView, perceptualRoughness, IN.lightmapUV,
                                s.Occlusion, diffuseColor, specColor, ltcResult);
            return ltcResult;
        }

        inline half3 LTCSpecular(Input IN, SurfaceOutputStandard s, float3 worldNormal, float3 worldView)
        {
            half3 specColor;
            half oneMinusReflectivity = 0;
            DiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, specColor, oneMinusReflectivity);
            half perceptualRoughness = SmoothnessToPerceptualRoughness(s.Smoothness);
            half3 ltcSpec;
            KanikamaLTCSpecular(IN.worldPos, worldNormal, worldView, perceptualRoughness, IN.lightmapUV,
                                s.Occlusion, specColor, ltcSpec);
            return ltcSpec;
        }

        inline void LightingKanikama_GI(SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi)
        {
            Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal,
                                                                        lerp(unity_ColorSpaceDielectricSpec.rgb,
                                                                             s.Albedo, s.Metallic));
            gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
            #if defined(_KANIKAMA_MODE_ARRAY) || defined(_KANIKAMA_MODE_DIRECTIONAL)
            half roughness = SmoothnessToRoughness(s.Smoothness);
            half3 diffuse;
            half3 specular;
            KanikamaGI(data.lightmapUV, s.Normal, data.worldViewDir, roughness, s.Occlusion, diffuse, specular);
            gi.indirect.diffuse += diffuse;
            gi.indirect.specular += specular;
            #endif
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

            #if defined(_KANIKAMA_LTC_DIFFUSE_SPECULAR)
            o.Emission += LTCDiffuseSpecular(IN, o, WorldNormalVector(IN, o.Normal),
                                      normalize(UnityWorldSpaceViewDir(IN.worldPos)));
            #elif defined(_KANIKAMA_LTC_SPECULAR)
            o.Emission += LTCSpecular(IN, o, WorldNormalVector(IN, o.Normal),
                                      normalize(UnityWorldSpaceViewDir(IN.worldPos)));
            #endif
        }
        ENDCG
    }
    FallBack "Diffuse"
    CustomEditor "Kanikama.Editor.Application.KanikamaStandardGUI"
}