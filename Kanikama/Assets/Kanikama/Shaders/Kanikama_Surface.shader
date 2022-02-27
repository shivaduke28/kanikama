Shader "Kanikama/Surface"
{
    Properties
    {
        [KeywordEnum(None, Single, Array, Directional, Directional_Specular)] _Kanikama_Mode("Kanikama Mode", Float) = 0

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
        [Header(Normal)]
        [Space]
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1.0

        [Space]
        [Header(Occlusion)]
        [Space]
        [NoScaleOffset] _OcclusionMap("Occlusion", 2D) = "white" {}
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0

        [Space]
        [Header(Emission)]
        [Space]
        [Toggle(_EMISSION)] _Emission("Emission", Float) = 0
        [NoScaleOffset] _EmissionMap("Emission Map", 2D) = "white" {}
        [HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #include "UnityStandardUtils.cginc"
        #include "./CGIncludes/KanikamaComposite.hlsl"

        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow 
        #pragma target 3.0

        #pragma shader_feature_local_fragment _ _KANIKAMA_MODE_SINGLE _KANIKAMA_MODE_ARRAY _KANIKAMA_MODE_DIRECTIONAL _KANIKAMA_MODE_DIRECTIONAL_SPECULAR
        #pragma shader_feature_local_fragment _ _EMISSION

        fixed4 _Color;
        sampler2D _MainTex;
        sampler2D _BumpMap;
        half _BumpScale;
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
            float3 worldNormal; INTERNAL_DATA
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.lightmapUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;
            fixed4 c = tex2D (_MainTex, uv) * _Color;
            o.Albedo = c.rgb;
            half2 mg = tex2D(_MetallicGlossMap, uv).ra;
            o.Metallic = mg.r * _Metallic;
            o.Smoothness = mg.g * _Glossiness;
            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, uv), _BumpScale);
            o.Occlusion = LerpOneTo(tex2D(_OcclusionMap, uv).g, _OcclusionStrength);

#if defined(_EMISSION)
            o.Emission = tex2D(_EmissionMap, uv).rgb * _EmissionColor;
#endif

            half3 specular;
            half oneMinusReflectivity;
            half3 diffuse = DiffuseAndSpecularFromMetallic(o.Albedo, o.Metallic, specular, oneMinusReflectivity);
            float3 normal = WorldNormalVector(IN, o.Normal);
#if defined(_KANIKAMA_MODE_SINGLE)
            o.Emission += diffuse * KanikamaSampleLightmap(IN.lightmapUV) * o.Occlusion;
#elif defined(_KANIKAMA_MODE_ARRAY)
            o.Emission += diffuse * KanikamaSampleLightmapArray(IN.lightmapUV) * o.Occlusion;
#elif defined(_KANIKAMA_MODE_DIRECTIONAL)
            o.Emission += diffuse * KanikamaSampleDirectionalLightmapArray(IN.lightmapUV, normal) * o.Occlusion;
#elif defined(_KANIKAMA_MODE_DIRECTIONAL_SPECULAR)
            half3 diff;
            half3 spec;
            half3 view = normalize(_WorldSpaceCameraPos - IN.worldPos);
            half roughness = SmoothnessToRoughness(o.Smoothness);
            KanikamaDirectionalLightmapSpecular(IN.lightmapUV, normal, view, roughness, o.Occlusion, diff, spec);
            o.Emission += diffuse * diff + specular * spec;
#endif

            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
