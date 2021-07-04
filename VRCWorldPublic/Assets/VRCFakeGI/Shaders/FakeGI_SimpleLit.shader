Shader "FakeGI/SimpleLit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LightMap("LightMap", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "SimpleLit.hlsl"

            ENDCG
        }
    }
}
