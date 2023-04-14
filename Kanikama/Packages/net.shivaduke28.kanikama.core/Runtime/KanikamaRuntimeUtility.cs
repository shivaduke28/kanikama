using UnityEngine;

namespace Kanikama.Core
{
    public static class KanikamaRuntimeUtility
    {
        public static void DestroySafe(Object obj)
        {
            if (obj == null) return;
#if UNITY_EDITOR
            Object.DestroyImmediate(obj);
#else
            Object.Destroy(obj);
#endif
        }

        public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            if (!gameObject.TryGetComponent<T>(out var component))
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        public static bool IsContributeGI(Material material)
        {
            return material.globalIlluminationFlags.HasFlag(MaterialGlobalIlluminationFlags.BakedEmissive);
        }

        public static void AddBakedEmissiveFlag(Material material)
        {
            var flags = material.globalIlluminationFlags;
            flags |= MaterialGlobalIlluminationFlags.BakedEmissive;
            material.globalIlluminationFlags = flags;
        }

        public static void RemoveBakedEmissiveFlag(Material material)
        {
            var flags = material.globalIlluminationFlags;
            flags &= ~MaterialGlobalIlluminationFlags.BakedEmissive;
            material.globalIlluminationFlags = flags;
        }
    }
}
