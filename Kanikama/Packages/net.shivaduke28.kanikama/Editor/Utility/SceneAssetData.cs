using UnityEditor;
using UnityEngine.SceneManagement;

namespace Kanikama.Editor.Utility
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

        public static bool TryFindFromActiveScene(out SceneAssetData sceneAssetData)
        {
            var scene = SceneManager.GetActiveScene();
            var path = scene.path;
            if (string.IsNullOrEmpty(path))
            {
                sceneAssetData = default;
                return false;
            }
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
            sceneAssetData = new SceneAssetData(sceneAsset);
            return true;
        }
    }
}
