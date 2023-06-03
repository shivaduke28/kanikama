using System;
using System.IO;
using Kanikama.Editor.Baking;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Bakery.Editor.Baking
{
    [Serializable]
    public class BakeryBakingSetting
    {
        [SerializeField] SceneAsset sceneAsset;
        [SerializeField] TextureResizeType textureResizeType = TextureResizeType.One;
        [SerializeField] BakeryLightmapStorage lightmapStorage = new BakeryLightmapStorage();
        [SerializeField] BakeryLightmapArrayStorage lightmapArrayStorage = new BakeryLightmapArrayStorage();
        [SerializeField] string outputAssetDirPath = "Assets";

        public SceneAsset SceneAsset => sceneAsset;
        public BakeryLightmapStorage LightmapStorage => lightmapStorage;
        public BakeryLightmapArrayStorage LightmapArrayStorage => lightmapArrayStorage;
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
