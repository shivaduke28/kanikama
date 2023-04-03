using UnityEditor;

namespace Kanikama.Core.Editor
{
    public static class ObjectUtility
    {
        public static bool TryCreateGlobalObjectId(string assetGUID, int identifierType, ulong targetObjectId, ulong targetPrefabId, out GlobalObjectId id)
        {
            id = default;
            var str = $"GlobalObjectId_V1-{identifierType}-{assetGUID}-{targetObjectId}-{targetPrefabId}";
            return GlobalObjectId.TryParse(str, out id);
        }
    }
}
