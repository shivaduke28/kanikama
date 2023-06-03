using UnityEditor;

namespace Kanikama.Editor.Baking.Util
{
    public static class GlobalObjectIdHelper
    {
        public static bool TryParse(string assetGUID, int identifierType, ulong targetObjectId, ulong targetPrefabId, out GlobalObjectId id)
        {
            id = default;
            var str = $"GlobalObjectId_V1-{identifierType}-{assetGUID}-{targetObjectId}-{targetPrefabId}";
            return GlobalObjectId.TryParse(str, out id);
        }
    }
}
