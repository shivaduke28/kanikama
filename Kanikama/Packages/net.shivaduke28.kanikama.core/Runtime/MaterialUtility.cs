using UnityEngine;

namespace Kanikama.Core
{
    public static class MaterialUtility
    {
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
