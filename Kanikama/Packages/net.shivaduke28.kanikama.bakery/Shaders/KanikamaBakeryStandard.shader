Shader "Kanikama/KanikamaBakeryStandard"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _DetailMask("Detail Mask", 2D) = "white" {}

        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        _DetailNormalMap("Normal Map", 2D) = "bump" {}

        [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0

        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0

        _Volume0("Volume0", 3D) = "black" {}
        _Volume1("Volume1", 3D) = "black" {}
        _Volume2("Volume2", 3D) = "black" {}
        _VolumeMask("Volume Mask", 3D) = "white" {}
        _VolumeMin("Volume min", Vector) = (0,0,0)
        _VolumeInvSize("Volume Inv Size", Vector) = (1000001, 1000001, 1000001)

        [HideInInspector] _BAKERY_2SIDED ("__2s", Float) = 2.0
        [Toggle(BAKERY_2SIDEDON)] _BAKERY_2SIDEDON ("Double-sided", Float) = 0
        [Toggle(BAKERY_VERTEXLM)] _BAKERY_VERTEXLM ("Enable vertex LM", Float) = 0
        [Toggle(BAKERY_VERTEXLMDIR)] _BAKERY_VERTEXLMDIR ("Enable directional vertex LM", Float) = 0
        [Toggle(BAKERY_VERTEXLMSH)] _BAKERY_VERTEXLMSH ("Enable SH vertex LM", Float) = 0
        [Toggle(BAKERY_VERTEXLMMASK)] _BAKERY_VERTEXLMMASK ("Enable shadowmask vertex LM", Float) = 0
        [Toggle(BAKERY_SH)] _BAKERY_SH ("Enable SH", Float) = 0
        [Toggle(BAKERY_SHNONLINEAR)] _BAKERY_SHNONLINEAR ("SH non-linear mode", Float) = 1
        [Toggle(BAKERY_RNM)] _BAKERY_RNM ("Enable RNM", Float) = 0
        [Toggle(BAKERY_MONOSH)] _BAKERY_MONOSH ("Enable MonoSH", Float) = 0
        [Toggle(BAKERY_LMSPEC)] _BAKERY_LMSPEC ("Enable Lightmap Specular", Float) = 0
        [Toggle(BAKERY_LMSPECOCCLUSION)] _BAKERY_LMSPECOCCLUSION ("Use Lightmap Specular as Reflection Occlusion", Float) = 0
        [Toggle(BAKERY_BICUBIC)] _BAKERY_BICUBIC ("Enable Bicubic Filter", Float) = 0
        [Toggle(BAKERY_PROBESHNONLINEAR)] _BAKERY_PROBESHNONLINEAR ("Use non-linear SH for light probes", Float) = 0
        [Toggle(BAKERY_VOLUME)] _BAKERY_VOLUME ("Use volumes", Float) = 0
        [Toggle(BAKERY_VOLROTATION)] _BAKERY_VOLROTATION ("Support volume rotation", Float) = 0

        [KeywordEnum(None, Array, Directional, Bakery MonoSH)] _Kanikama_Mode("Kanikama Mode", Float) = 0
        [Toggle(_KANIKAMA_DIRECTIONAL_SPECULAR)] _Kanikama_Directional_Specular("Kanikama Directional Specular", Float) = 0
        [Toggle(_KANIKAMA_BAKERY_SHNONLINEAR)] _Kanikama_Bakery_SHNonlinear ("Kanikama SH non-linear mode", Float) = 1
        [PerRendererData]_Udon_LightmapArray("LightmapArray", 2DArray) = ""{}
        [PerRendererData]_Udon_LightmapIndArray("LightmapIndArray", 2DArray) = ""{}
        [KeywordEnum(None, Specular, Diffuse Specular)] _Kanikama_LTC("Kanikama LTC", Float) = 0
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" "KanikamaGI"="True" }
        LOD 300


        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_BAKERY_2SIDED]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature _PARALLAXMAP
            #pragma shader_feature UNITY_SPECCUBE_BOX_PROJECTION

            #pragma shader_feature BAKERY_VERTEXLM
            #pragma shader_feature BAKERY_VERTEXLMDIR
            #pragma shader_feature BAKERY_VERTEXLMSH
            #pragma shader_feature BAKERY_VERTEXLMMASK
            #pragma shader_feature BAKERY_SH
            #pragma shader_feature BAKERY_MONOSH
            #pragma shader_feature BAKERY_SHNONLINEAR
            #pragma shader_feature BAKERY_RNM
            #pragma shader_feature BAKERY_LMSPEC
            #pragma shader_feature BAKERY_LMSPECOCCLUSION
            #pragma shader_feature BAKERY_BICUBIC
            #pragma shader_feature BAKERY_PROBESHNONLINEAR
            #pragma shader_feature BAKERY_VOLUME
            #pragma shader_feature BAKERY_VOLROTATION

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile _ BAKERY_COMPRESSED_VOLUME

            #pragma shader_feature_local_fragment _ _KANIKAMA_MODE_ARRAY _KANIKAMA_MODE_DIRECTIONAL _KANIKAMA_MODE_BAKERY_MONOSH
            #pragma shader_feature_local_fragment _ _KANIKAMA_DIRECTIONAL_SPECULAR
            #pragma shader_feature_local_fragment _ _KANIKAMA_BAKERY_SHNONLINEAR
            #pragma shader_feature_local_fragment _ _KANIKAMA_LTC_SPECULAR _KANIKAMA_LTC_DIFFUSE_SPECULAR

            #pragma vertex bakeryVertForwardBase
            #pragma fragment KanikamaBakeryFragForwardBase

            #include "UnityStandardCoreForward.cginc"
            #include "Assets/Bakery/shader/Bakery.cginc"
            #include "./KanikamaBakeryStandard.hlsl"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex bakeryVertForwardAdd
            #pragma fragment bakeryFragForwardAdd

            #include "UnityStandardCoreForward.cginc"
            #include "Assets/Bakery/shader/Bakery.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------

            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _PARALLAXMAP
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Deferred pass
        Pass
        {
            Name "DEFERRED"
            Tags { "LightMode" = "Deferred" }

            Cull [_BAKERY_2SIDED]

            CGPROGRAM
            #pragma target 3.0
            #pragma exclude_renderers nomrt


            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP

            #pragma shader_feature BAKERY_VERTEXLM
            #pragma shader_feature BAKERY_VERTEXLMDIR
            #pragma shader_feature BAKERY_VERTEXLMSH
            #pragma shader_feature BAKERY_VERTEXLMMASK
            #pragma shader_feature BAKERY_SH
            #pragma shader_feature BAKERY_MONOSH
            #pragma shader_feature BAKERY_SHNONLINEAR
            #pragma shader_feature BAKERY_RNM
            #pragma shader_feature BAKERY_LMSPEC
            #pragma shader_feature BAKERY_BICUBIC
            #pragma shader_feature BAKERY_PROBESHNONLINEAR
            #pragma shader_feature BAKERY_VOLUME
            #pragma shader_feature BAKERY_VOLROTATION

            #pragma multi_compile_prepassfinal
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile _ BAKERY_COMPRESSED_VOLUME

            #pragma vertex bakeryVertDeferred
            #pragma fragment bakeryFragDeferred

            #include "UnityStandardCore.cginc"
            #include "Assets/Bakery/shader/Bakery.cginc"

            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 150

        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_BAKERY_2SIDED]

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF

            //#pragma shader_feature BAKERY_VERTEXLM
            //#pragma shader_feature BAKERY_VERTEXLMDIR
            //#pragma shader_feature BAKERY_VERTEXLMSH
            //#pragma shader_feature BAKERY_VERTEXLMMASK
            //#pragma shader_feature BAKERY_SH
            //#pragma shader_feature BAKERY_MONOSH
            //#pragma shader_feature BAKERY_SHNONLINEAR
            //#pragma shader_feature BAKERY_RNM
            //#pragma shader_feature BAKERY_LMSPEC
            //#pragma shader_feature BAKERY_BICUBIC

            // SM2.0: NOT SUPPORTED shader_feature ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP

            #pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #pragma vertex bakeryVertForwardBase
            #pragma fragment bakeryFragForwardBase

            #include "UnityStandardCoreForward.cginc"
            #include "Assets/Bakery/shader/Bakery.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP
            #pragma skip_variants SHADOWS_SOFT

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog

            #pragma vertex bakeryVertForwardAdd
            #pragma fragment bakeryFragForwardAdd

            #include "UnityStandardCoreForward.cginc"
            #include "Assets/Bakery/shader/Bakery.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma skip_variants SHADOWS_SOFT
            #pragma multi_compile_shadowcaster

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"

            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }


    FallBack "VertexLit"
    CustomEditor "Kanikama.Bakery.Editor.GUI.KanikamaBakeryStandardGUI"
}
