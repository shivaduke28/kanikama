using UnityEditor;
using UnityEngine;

namespace Kanikama.Core.Editor
{
    public readonly struct SceneAssetData
    {
        public string Path { get; }
        public SceneAsset Asset { get; }

        public string LightingAssetDirectoryPath { get; }
        // TODO: Add GUID

        public SceneAssetData(string path, SceneAsset asset, string lightingAssetDirectoryPath)
        {
            Path = path;
            Asset = asset;
            LightingAssetDirectoryPath = lightingAssetDirectoryPath;
        }
    }
}
