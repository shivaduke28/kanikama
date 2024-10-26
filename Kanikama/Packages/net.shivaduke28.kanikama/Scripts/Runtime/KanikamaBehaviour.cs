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
    }
}
