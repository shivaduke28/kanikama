using UnityEditor;

namespace Kanikama.Editor
{
    public static class GlobalObjectIdUtility
    {
        public static bool TryParse(string assetGUID, int identifierType, ulong targetObjectId, ulong targetPrefabId, out GlobalObjectId id)
        {
            id = default;
            var str = $"GlobalObjectId_V1-{identifierType}-{assetGUID}-{targetObjectId}-{targetPrefabId}";
            return GlobalObjectId.TryParse(str, out id);
        }
    }
}
