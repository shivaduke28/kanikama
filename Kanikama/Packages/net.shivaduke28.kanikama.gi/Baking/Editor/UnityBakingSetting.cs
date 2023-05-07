using System;
using System.IO;
using Kanikama.Core.Editor;
using Kanikama.Core.Editor.Util;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Editor
{
    [Serializable]
    public sealed class UnityBakingSetting
    {
        [SerializeField] SceneAsset sceneAsset;
        [SerializeField] TextureResizeType textureResizeType = TextureResizeType.One;
        [SerializeField] UnityLightmapStorage lightmapStorage;
        [SerializeField] UnityLightmapArrayStorage lightmapArrayStorage;
        [SerializeField] string outputAssetDirPath = "Assets";

        public SceneAsset SceneAsset
        {
            get => sceneAsset;
            set
            {
                sceneAsset = value;
                outputAssetDirPath = GetOutputAssetDirPath(value);
            }
        }

        public TextureResizeType TextureResizeType => textureResizeType;
        public UnityLightmapStorage LightmapStorage => lightmapStorage;
        public UnityLightmapArrayStorage LightmapArrayStorage => lightmapArrayStorage;
        public string OutputAssetDirPath => outputAssetDirPath;


        public UnityBakingSetting(SceneAsset sceneAsset, TextureResizeType textureResizeType)
        {
            SceneAsset = sceneAsset;
            this.textureResizeType = textureResizeType;
        }

        public UnityBakingSetting Clone()
        {
            return new UnityBakingSetting(sceneAsset, textureResizeType);
        }

        public static string GetOutputAssetDirPath(SceneAsset sceneAsset)
        {
            var path = AssetDatabase.GetAssetPath(sceneAsset);
            var dirPath = Path.GetDirectoryName(path);
            return dirPath != null ? Path.Combine(dirPath, $"{sceneAsset.name}_kanikama_unity") : string.Empty;
        }
    }
}
