// This is a modification of UnityStandardCore.cginc from
// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef KANIKAMA_STANDARD_CORE_INCLUDED
    #define KANIKAMA_STANDARD_CORE_INCLUDED

    #include "UnityStandardCore.cginc"
    #include "UnityStandardConfig.cginc"
    #include "KanikamaGlobalIllumination.hlsl"

    #if defined(UNITY_NO_FULL_STANDARD_SHADER)
        #define UNITY_STANDARD_SIMPLE 1
    #endif

    inline UnityGI FragmentKanikamaGI(FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light)
    {
        UnityGIInput d;
        d.light = light;
        d.worldPos = s.posWorld;
        d.worldViewDir = -s.eyeVec;
        d.atten = atten;
        #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
            d.ambient = 0;
            d.lightmapUV = i_ambientOrLightmapUV;
        #else
            d.ambient = i_ambientOrLightmapUV.rgb;
            d.lightmapUV = 0;
        #endif

        d.probeHDR[0] = unity_SpecCube0_HDR;
        d.probeHDR[1] = unity_SpecCube1_HDR;
        #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
            d.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
        #endif
        #ifdef UNITY_SPECCUBE_BOX_PROJECTION
            d.boxMax[0] = unity_SpecCube0_BoxMax;
            d.probePosition[0] = unity_SpecCube0_ProbePosition;
            d.boxMax[1] = unity_SpecCube1_BoxMax;
            d.boxMin[1] = unity_SpecCube1_BoxMin;
            d.probePosition[1] = unity_SpecCube1_ProbePosition;
        #endif

        Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.smoothness, -s.eyeVec, s.normalWorld, s.specColor);
        // Replace the reflUVW if it has been compute in Vertex shader. Note: the compiler will optimize the calcul in UnityGlossyEnvironmentSetup itself
        #if UNITY_STANDARD_SIMPLE
            g.reflUVW = s.reflUVW;
        #endif

        return KanikamaGlobalIllumination(d, occlusion, s.normalWorld, g);
    }

    half4 fragKanikamaForwardBaseInternal(VertexOutputForwardBase i)
    {
        UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

        FRAGMENT_SETUP(s)

        UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

        UnityLight mainLight = MainLight();
        UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld);

        half occlusion = Occlusion(i.tex.xy);
        UnityGI gi = FragmentKanikamaGI(s, occlusion, i.ambientOrLightmapUV, atten, mainLight);

        half4 c = UNITY_BRDF_PBS(s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);
        c.rgb += Emission(i.tex.xy);

        UNITY_EXTRACT_FOG_FROM_EYE_VEC(i);
        UNITY_APPLY_FOG(_unity_fogCoord, c.rgb);
        return OutputForward(c, s.alpha);
    }


#endif // KANIKAMA_STANDARD_CORE_INCLUDED
