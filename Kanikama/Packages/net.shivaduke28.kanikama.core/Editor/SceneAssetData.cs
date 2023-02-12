using UnityEditor;
using UnityEngine;

namespace Kanikama.Core.Editor
{
    public readonly struct SceneAssetData
    {
        public string Path { get; }
        public SceneAsset Asset { get; }
        public string LightingAssetDirectoryPath { get; }

        public SceneAssetData(string path, SceneAsset asset, string lightingAssetDirectoryPath)
        {
            Path = path;
            Asset = asset;
            LightingAssetDirectoryPath = lightingAssetDirectoryPath;
        }
    }

    public readonly struct AssetData<T> where T : Object
    {
        public string Path { get; }
        public T Asset { get; }

        public AssetData(string path, T asset)
        {
            Path = path;
            Asset = asset;
        }
    }
}
