using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor
{
    [Serializable]
    public sealed class UnityBakingSetting
    {
        [SerializeField] SceneAsset sceneAsset;
        [SerializeField] TextureResizeType textureResizeType = TextureResizeType.One;
        [SerializeField] string outputAssetDirPath = "Assets";
        [SerializeField] AssetStorage assetStorage;

        public SceneAsset SceneAsset => sceneAsset;
        public TextureResizeType TextureResizeType => textureResizeType;
        public string OutputAssetDirPath => outputAssetDirPath;
        public AssetStorage AssetStorage => assetStorage;


        public UnityBakingSetting(SceneAsset sceneAsset, TextureResizeType textureResizeType, string outputDirSuffix = "_kanikama_unity")
        {
            this.sceneAsset = sceneAsset;
            this.textureResizeType = textureResizeType;
            var path = AssetDatabase.GetAssetPath(sceneAsset);
            var dirPath = Path.GetDirectoryName(path);
            outputAssetDirPath = dirPath != null ? Path.Combine(dirPath, $"{sceneAsset.name}{outputDirSuffix}") : string.Empty;
        }

        public UnityBakingSetting Clone()
        {
            return new UnityBakingSetting(sceneAsset, textureResizeType);
        }
    }
}
