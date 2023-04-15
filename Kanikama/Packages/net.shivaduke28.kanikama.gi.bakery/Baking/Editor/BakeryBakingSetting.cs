using System;
using System.IO;
using Kanikama.Core.Editor.Textures;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Bakery.Editor
{
    [Serializable]
    public class BakeryBakingSetting
    {
        [SerializeField] SceneAsset sceneAsset;
        [SerializeField] TextureResizeType textureResizeType = TextureResizeType.One;
        [SerializeField] string outputAssetDirPath = "Assets";
        [SerializeField] BakeryLightmapStorage bakeryLightmapStorage = new BakeryLightmapStorage();

        public SceneAsset SceneAsset => sceneAsset;
        public BakeryLightmapStorage LightmapStorage => bakeryLightmapStorage;
        public TextureResizeType TextureResizeType => textureResizeType;
        public string OutputAssetDirPath => outputAssetDirPath;


        public void SetSceneAsset(SceneAsset scene)
        {
            sceneAsset = scene;
            outputAssetDirPath = GetOutputAssetDirPath(scene);
        }

        public static string GetOutputAssetDirPath(SceneAsset sceneAsset)
        {
            var path = AssetDatabase.GetAssetPath(sceneAsset);
            var dirPath = Path.GetDirectoryName(path);
            return dirPath != null ? Path.Combine(dirPath, $"{sceneAsset.name}_kanikama_bakery") : string.Empty;
        }
    }
}
