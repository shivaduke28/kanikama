using UnityEngine;

namespace Kanikama
{
    public abstract class KanikamaBehaviour :
#if UDONSHARP
        UdonSharp.UdonSharpBehaviour
#else
        UnityEngine.MonoBehaviour
#endif
    {
    }

    public static class KanikamaShader
    {
        public static int PropertyToID(string name)
        {
#if UDONSHARP
            return VRC.SDKBase.VRCShader.PropertyToID(name);
#else
            return UnityEngine.Shader.PropertyToID(name);
#endif
        }

        public static void SetGlobalVectorArray(int id, Vector4[] value)
        {
#if UDONSHARP
            VRC.SDKBase.VRCShader.SetGlobalVectorArray(id, value);
#else
            UnityEngine.Shader.SetGlobalVectorArray(id, value);
#endif
        }

        public static void SetGlobalInteger(int id, int value)
        {
#if UDONSHARP
            VRC.SDKBase.VRCShader.SetGlobalInteger(id, value);
#else
            UnityEngine.Shader.SetGlobalInteger(id, value);
#endif
        }
        
        public static void SetGlobalTexture(int id, Texture value)
        {
#if UDONSHARP
            VRC.SDKBase.VRCShader.SetGlobalTexture(id, value);
#else
            UnityEngine.Shader.SetGlobalTexture(id, value);
#endif
        }
    }
}
