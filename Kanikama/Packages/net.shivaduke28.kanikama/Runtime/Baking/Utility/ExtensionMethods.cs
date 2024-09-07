using UnityEngine;

namespace Kanikama.Utility
{
    public static class ExtensionMethods
    {
        public static void DestroySafely(this Object obj)
        {
            if (obj == null) return;
#if UNITY_EDITOR
            if (UnityEngine.Application.isPlaying)
            {
                Object.Destroy(obj);
            }
            else
            {
                Object.DestroyImmediate(obj);
            }
#else
            Object.Destroy(obj);
#endif
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject.TryGetComponent<T>(out var component))
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        public static void AddBakedEmissiveFlag(this Material material)
        {
            var flags = material.globalIlluminationFlags;
            flags |= MaterialGlobalIlluminationFlags.BakedEmissive;
            material.globalIlluminationFlags = flags;
        }

        public static void RemoveBakedEmissiveFlag(this Material material)
        {
            var flags = material.globalIlluminationFlags;
            flags &= ~MaterialGlobalIlluminationFlags.BakedEmissive;
            material.globalIlluminationFlags = flags;
        }

        public static bool IsEmissive(this Material material)
        {
            return material.globalIlluminationFlags.HasFlag(MaterialGlobalIlluminationFlags.BakedEmissive);
        }

        public static bool IsContributeGI(this Light light)
        {
            return light.isActiveAndEnabled && (light.bakingOutput.lightmapBakeType & LightmapBakeType.Realtime) == 0;
        }
    }
}
