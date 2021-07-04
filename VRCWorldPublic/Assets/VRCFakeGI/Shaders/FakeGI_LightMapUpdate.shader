Shader "FakeGI/LightMapUpdate"
{
    Properties
    {
        _LightMap1 ("LightMap", 2D) = "black" {}
        _Color1("Color", Color) = (1,1,1,1)
        _Intensity1 ("Intensity", Float) = 1

        _LightMap2 ("LightMap", 2D) = "black" {}
        _Color2("Color", Color) = (1,1,1,1)
        _Intensity2("Intensity", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {

            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            sampler2D _LightMap1;
            float4 _Color1;
            float _Intensity1;

            sampler2D _LightMap2;
            float4 _Color2;
            float _Intensity2;

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                float2 uv = IN.localTexcoord.xy;
                return tex2D(_LightMap1, uv) * _Color1 * _Intensity1
                     + tex2D(_LightMap2, uv) * _Color2 * _Intensity2;
            }
            ENDCG
        }
    }
}
