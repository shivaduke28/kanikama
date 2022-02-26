Shader "Kanikama/Surface"
{
    Properties
    {
        [KeywordEnum(None, Single, Array, Directional)] _Kanikama_Mode("Kanikama Mode", Float) = 0
        [Space]

        _Color("Color", Color) = (1, 1, 1, 1)
        _MainTex("Albedo", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #include "UnityStandardUtils.cginc"
        #include "../CGIncludes/KanikamaComposite.hlsl"
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        #pragma shader_feature _ _KANIKAMA_MODE_SINGLE _KANIKAMA_MODE_ARRAY _KANIKAMA_MODE_DIRECTIONAL 

        fixed4 _Color;
        sampler2D _MainTex;
        sampler2D _BumpMap;
        half _Glossiness;
        half _Metallic;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
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
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            float3 normal = WorldNormalVector(IN, o.Normal);

#if defined(_KANIKAMA_MODE_SINGLE) || defined(_KANIKAMA_MODE_ARRAY) || defined(_KANIKAMA_MODE_DIRECTIONAL)
            half3 kanikamaDiffuse;
#if defined(_KANIKAMA_MODE_SINGLE)
            kanikamaDiffuse = KanikamaSampleLightmap(IN.lightmapUV);
#elif defined(_KANIKAMA_MODE_ARRAY)
            kanikamaDiffuse = KanikamaSampleLightmapArray(IN.lightmapUV);
#elif defined(_KANIKAMA_MODE_DIRECTIONAL)
            kanikamaDiffuse = KanikamaSampleDirectionalLightmapArray(IN.lightmapUV, IN.worldNormal);
#endif
            half3 diffColor = o.Albedo * OneMinusReflectivityFromMetallic(o.Metallic);
            o.Emission = diffColor * kanikamaDiffuse;
#endif 

            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
