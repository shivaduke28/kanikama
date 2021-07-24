// This is a modification of UnityStandardCoreForward.cginc from
// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef KANIKAMA_STANDARD_CORE_FORWARD_INCLUDED
    #define KANIKAMA_STANDARD_CORE_FORWARD_INCLUDED

    #if defined(UNITY_NO_FULL_STANDARD_SHADER)
        #   define UNITY_STANDARD_SIMPLE 1
    #endif

    #include "UnityStandardConfig.cginc"

    #if UNITY_STANDARD_SIMPLE
        // #include "UnityStandardCoreForwardSimple.cginc"
        // VertexOutputBaseSimple vertBase (VertexInput v) { return vertForwardBaseSimple(v); }
        // VertexOutputForwardAddSimple vertAdd (VertexInput v) { return vertForwardAddSimple(v); }
        // half4 fragBase (VertexOutputBaseSimple i) : SV_Target { return fragForwardBaseSimpleInternal(i); }
        // half4 fragAdd (VertexOutputForwardAddSimple i) : SV_Target { return fragForwardAddSimpleInternal(i); }
    #else
        #include "KanikamaStandardCore.hlsl"
        VertexOutputForwardBase vertBase (VertexInput v) { return vertForwardBase(v); }
        VertexOutputForwardAdd vertAdd (VertexInput v) { return vertForwardAdd(v); }
        half4 fragBase (VertexOutputForwardBase i) : SV_Target { return fragKanikamaForwardBaseInternal(i); }
        half4 fragAdd (VertexOutputForwardAdd i) : SV_Target { return fragForwardAddInternal(i); }
    #endif

#endif // KANIKAMA_STANDARD_CORE_FORWARD_INCLUDED
