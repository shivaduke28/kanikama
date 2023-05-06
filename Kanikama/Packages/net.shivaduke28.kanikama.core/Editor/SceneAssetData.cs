using UnityEditor;

namespace Kanikama.Core.Editor
{
    public readonly struct SceneAssetData
    {
        public SceneAsset Asset { get; }
        public string Path { get; }
        public string Guid { get; }

        public string LightingAssetDirectoryPath { get; }

        public SceneAssetData(SceneAsset sceneAsset)
        {
            Asset = sceneAsset;
            Path = AssetDatabase.GetAssetPath(Asset);
            Guid = AssetDatabase.AssetPathToGUID(Path);

            var assetDirPath = System.IO.Path.GetDirectoryName(Path);
            LightingAssetDirectoryPath = assetDirPath != null ? System.IO.Path.Combine(assetDirPath, sceneAsset.name) : string.Empty;
        }
    }
}
