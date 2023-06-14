using System;
using System.IO;
using Kanikama.Editor.Baking;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.Baking
{
    [Serializable]
    public sealed class UnityBakingSetting
    {
        [SerializeField] SceneAsset sceneAsset;
        [SerializeField] TextureResizeType textureResizeType = TextureResizeType.One;
        [SerializeField] UnityLightmapStorage lightmapStorage;
        [SerializeField] UnityLightmapArrayStorage lightmapArrayStorage;
        [SerializeField] string outputAssetDirPath = "Assets";

        public SceneAsset SceneAsset => sceneAsset;
        public TextureResizeType TextureResizeType => textureResizeType;
        public UnityLightmapStorage LightmapStorage => lightmapStorage;
        public UnityLightmapArrayStorage LightmapArrayStorage => lightmapArrayStorage;
        public string OutputAssetDirPath => outputAssetDirPath;


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
