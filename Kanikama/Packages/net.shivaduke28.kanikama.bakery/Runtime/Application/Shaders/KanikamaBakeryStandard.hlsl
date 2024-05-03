#ifndef KANIKAMA_BAKERY_STANDARD_INCLUDED
#define KANIKAMA_BAKERY_STANDARD_INCLUDED

#include "Assets/Bakery/shader/Bakery.cginc"
#include "UnityStandardCoreForward.cginc"
#include "KanikamaBakery.hlsl"
#include "Packages/net.shivaduke28.kanikama/Runtime/Application/Shaders/KanikamaLTC.hlsl"

half4 KanikamaBakeryFragForwardBase(BakeryVertexOutputForwardBase i) : SV_Target
{
    FRAGMENT_SETUP(s)
    UNITY_SETUP_INSTANCE_ID(i);
    #if UNITY_OPTIMIZE_TEXCUBELOD
    s.reflUVW = i.reflUVW;
    #endif

    UnityLight mainLight = MainLight();
    UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld);

    #ifdef BAKERY_VOLUME
    bool isGlobal = _VolumeInvSize.x > 1000000; // ~inf
    float3 volViewDir = s.eyeVec;
    #ifdef BAKERY_VOLROTATION
    float4x4 volMatrix = (isGlobal ? _GlobalVolumeMatrix : _VolumeMatrix);
    float3 volInvSize = (isGlobal ? _GlobalVolumeInvSize : _VolumeInvSize);
    float3 lpUV = mul(volMatrix, float4(s.posWorld, 1)).xyz * volInvSize + 0.5f;
    float3 volNormal = mul((float3x3)volMatrix, s.normalWorld);
    #ifdef BAKERY_LMSPEC
    volViewDir = mul((float3x3)volMatrix, volViewDir);
    #endif
    #else
    float3 lpUV = (s.posWorld - (isGlobal ? _GlobalVolumeMin : _VolumeMin)) * (isGlobal ? _GlobalVolumeInvSize : _VolumeInvSize);
    float3 volNormal = s.normalWorld;
    #endif
    #endif

    #ifdef BAKERY_VOLUME
    mainLight.color *= saturate(dot(_VolumeMask.Sample(sampler_Volume0, lpUV), unity_OcclusionMaskSelector));
    #elif BAKERY_VERTEXLMMASK
    if (bakeryLightmapMode == BAKERYMODE_VERTEXLM)
    {
        mainLight.color *= saturate(dot(i.ambientOrLightmapUV, unity_OcclusionMaskSelector));
    }
    #endif

    half occlusion = Occlusion(i.tex.xy);
    UnityGI gi = FragmentGI(s, occlusion, i.ambientOrLightmapUV, atten, mainLight);

    #ifdef BAKERY_VOLUME

    #ifdef BAKERY_COMPRESSED_VOLUME
    float4 tex0, tex1, tex2, tex3;
    float3 L0, L1x, L1y, L1z;
    tex0 = _Volume0.Sample(sampler_Volume0, lpUV);
    tex1 = _Volume1.Sample(sampler_Volume0, lpUV) * 2 - 1;
    tex2 = _Volume2.Sample(sampler_Volume0, lpUV) * 2 - 1;
    tex3 = _Volume3.Sample(sampler_Volume0, lpUV) * 2 - 1;

    #ifdef BAKERY_COMPRESSED_VOLUME_RGBM
    L0 = tex0.xyz * (tex0.w * 8.0f);
    L0 *= L0;
    #else
    L0 = tex0.xyz;
    #endif

    L1x = tex1.xyz * L0 * 2;
    L1y = tex2.xyz * L0 * 2;
    L1z = tex3.xyz * L0 * 2;
    #else

    float4 tex0, tex1, tex2;
    float3 L0, L1x, L1y, L1z;
    tex0 = _Volume0.Sample(sampler_Volume0, lpUV);
    tex1 = _Volume1.Sample(sampler_Volume0, lpUV);
    tex2 = _Volume2.Sample(sampler_Volume0, lpUV);
    L0 = tex0.xyz;
    L1x = tex1.xyz;
    L1y = tex2.xyz;
    L1z = float3(tex0.w, tex1.w, tex2.w);
    #endif

    gi.indirect.diffuse.r = shEvaluateDiffuseL1Geomerics(L0.r, float3(L1x.r, L1y.r, L1z.r), volNormal);
    gi.indirect.diffuse.g = shEvaluateDiffuseL1Geomerics(L0.g, float3(L1x.g, L1y.g, L1z.g), volNormal);
    gi.indirect.diffuse.b = shEvaluateDiffuseL1Geomerics(L0.b, float3(L1x.b, L1y.b, L1z.b), volNormal);

    #ifdef UNITY_COLORSPACE_GAMMA
    gi.indirect.diffuse = pow(gi.indirect.diffuse, 1.0f / 2.2f);
    #endif

    #ifdef BAKERY_LMSPEC
    float3 nL1x = L1x / L0;
    float3 nL1y = L1y / L0;
    float3 nL1z = L1z / L0;
    float3 dominantDir = float3(dot(nL1x, lumaConv), dot(nL1y, lumaConv), dot(nL1z, lumaConv));
    half3 halfDir = Unity_SafeNormalize(normalize(dominantDir) - volViewDir);
    half nh = saturate(dot(volNormal, halfDir));
    half perceptualRoughness = SmoothnessToPerceptualRoughness(s.smoothness);
    half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    half spec = GGXTerm(nh, roughness);
    float3 sh = L0 + dominantDir.x * L1x + dominantDir.y * L1y + dominantDir.z * L1z;
    #ifdef BAKERY_LMSPECOCCLUSION
    gi.indirect.specular *= saturate(dot(spec * sh, BAKERY_LMSPECOCCLUSION_MUL));
    #else
    gi.indirect.specular += max(spec * sh, 0.0);
    #endif
    #endif

    #elif BAKERY_PROBESHNONLINEAR
    float3 L0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
    gi.indirect.diffuse.r = shEvaluateDiffuseL1Geomerics(L0.r, unity_SHAr.xyz, s.normalWorld);
    gi.indirect.diffuse.g = shEvaluateDiffuseL1Geomerics(L0.g, unity_SHAg.xyz, s.normalWorld);
    gi.indirect.diffuse.b = shEvaluateDiffuseL1Geomerics(L0.b, unity_SHAb.xyz, s.normalWorld);
    #endif

    #ifdef DIRLIGHTMAP_COMBINED
    #ifdef BAKERY_LMSPEC
    #ifndef BAKERY_MONOSH
    if (bakeryLightmapMode == BAKERYMODE_DEFAULT)
    {
        float3 spec = BakeryDirectionalLightmapSpecular(i.ambientOrLightmapUV.xy, s.normalWorld, s.eyeVec, s.smoothness) * gi.indirect.diffuse;
    #ifdef BAKERY_LMSPECOCCLUSION
        gi.indirect.specular *= saturate(dot(spec, BAKERY_LMSPECOCCLUSION_MUL));
    #else
        gi.indirect.specular += spec;
    #endif
    }
    #endif
    #endif
    #endif

    #ifdef BAKERY_VERTEXLM
    if (bakeryLightmapMode == BAKERYMODE_VERTEXLM)
    {
        gi.indirect.diffuse = i.color.rgb;
        float3 prevSpec = gi.indirect.specular;

        #if defined(BAKERY_VERTEXLMDIR)

        #ifdef BAKERY_MONOSH
        BakeryVertexLMMonoSH(gi.indirect.diffuse, gi.indirect.specular, i.lightDirection, s.normalWorld, s.eyeVec,
                             s.smoothness);
        #else
        BakeryVertexLMDirection(gi.indirect.diffuse, gi.indirect.specular, i.lightDirection, i.tangentToWorldAndPackedData[2].xyz, s.normalWorld, s.eyeVec, s.smoothness);
        #endif

        #ifdef BAKERY_LMSPECOCCLUSION
        gi.indirect.specular = saturate(dot(gi.indirect.specular, BAKERY_LMSPECOCCLUSION_MUL)) * prevSpec;
        #else
        gi.indirect.specular += prevSpec;
        #endif

        #elif defined (BAKERY_VERTEXLMSH)
        BakeryVertexLMSH(gi.indirect.diffuse, gi.indirect.specular, i.shL1x, i.shL1y, i.shL1z, s.normalWorld, s.eyeVec, s.smoothness);

        #ifdef BAKERY_LMSPECOCCLUSION
        gi.indirect.specular = saturate(dot(gi.indirect.specular, BAKERY_LMSPECOCCLUSION_MUL)) * prevSpec;
        #else
        gi.indirect.specular += prevSpec;
        #endif

        #endif
    }
    #endif

    #ifdef BAKERY_RNM
    if (bakeryLightmapMode == BAKERYMODE_RNM)
    {
        #ifdef BAKERY_SSBUMP
            float3 normalMap = tex2D(_BumpMap, i.tex.xy).xyz;
        #else
        float3 normalMap = NormalInTangentSpace(i.tex);
        #endif

        float3 eyeVecT = 0;
        #ifdef BAKERY_LMSPEC
        eyeVecT = -NormalizePerPixelNormal(i.viewDirForParallax);
        #endif

        float3 prevSpec = gi.indirect.specular;
        BakeryRNM(gi.indirect.diffuse, gi.indirect.specular, i.ambientOrLightmapUV.xy, normalMap, s.smoothness,
                  eyeVecT);
        #ifdef BAKERY_LMSPECOCCLUSION
        gi.indirect.specular = saturate(dot(gi.indirect.specular, BAKERY_LMSPECOCCLUSION_MUL)) * prevSpec;
        #else
        gi.indirect.specular += prevSpec;
        #endif
    }
    #endif

    #ifdef BAKERY_SH
    #if SHADER_TARGET >= 30
    if (bakeryLightmapMode == BAKERYMODE_SH)
    #endif
    {
        float3 prevSpec = gi.indirect.specular;
        BakerySH(gi.indirect.diffuse, gi.indirect.specular, i.ambientOrLightmapUV.xy, s.normalWorld, s.eyeVec,
                 s.smoothness);
        #ifdef BAKERY_LMSPECOCCLUSION
        gi.indirect.specular = saturate(dot(gi.indirect.specular, BAKERY_LMSPECOCCLUSION_MUL)) * prevSpec;
        #else
        gi.indirect.specular += prevSpec;
        #endif
    }
    #endif

    #ifdef DIRLIGHTMAP_COMBINED
    #ifdef BAKERY_MONOSH
    if (bakeryLightmapMode != BAKERYMODE_VERTEXLM)
    {
        float3 prevSpec = gi.indirect.specular;
        BakeryMonoSH(gi.indirect.diffuse, gi.indirect.specular, i.ambientOrLightmapUV.xy, s.normalWorld, s.eyeVec, s.smoothness);
    #ifdef BAKERY_LMSPECOCCLUSION
        gi.indirect.specular = saturate(dot(gi.indirect.specular, BAKERY_LMSPECOCCLUSION_MUL)) * prevSpec;
    #else
        gi.indirect.specular += prevSpec;
    #endif
    }
    #endif
    #endif

    #if defined(_KANIKAMA_MODE_ARRAY) || defined(_KANIKAMA_MODE_DIRECTIONAL) || defined(_KANIKAMA_MODE_BAKERY_MONOSH)
    half3 kanikamaDiffuse;
    half3 kanikamaSpecular;
    KanikamaBakeryGI(i.ambientOrLightmapUV.xy, s.normalWorld, -s.eyeVec, s.smoothness, occlusion, kanikamaDiffuse,
                     kanikamaSpecular);
    gi.indirect.diffuse += kanikamaDiffuse;
    gi.indirect.specular += kanikamaSpecular;
    #endif

    half4 c = UNITY_BRDF_PBS(s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec,
                             gi.light, gi.indirect);

    c.rgb += UNITY_BRDF_GI(s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec,
                           occlusion, gi);

    #if defined(_KANIKAMA_LTC)
    half3 ltcSpec;
    KanikamaLTCSpecular(s.posWorld, s.normalWorld, -s.eyeVec, SmoothnessToPerceptualRoughness(s.smoothness), i.ambientOrLightmapUV.xy, occlusion, s.specColor, ltcSpec);
    c.rgb += ltcSpec;
    #endif

    c.rgb += Emission(i.tex.xy);

    UNITY_APPLY_FOG(i.fogCoord, c.rgb);

    return OutputForward(c, s.alpha);
}

#endif
