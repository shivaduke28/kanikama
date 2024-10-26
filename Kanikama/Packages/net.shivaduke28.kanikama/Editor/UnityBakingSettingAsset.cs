using System;
using System.IO;
using Kanikama.Editor.Utility;
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

    [CreateAssetMenu(menuName = "Kanikama/UnityBakingSettingAsset", fileName = "UnityBakingSettingAsset")]
    public sealed class UnityBakingSettingAsset : ScriptableObject
    {
        [SerializeField] UnityBakingSetting setting;

        public UnityBakingSetting Setting
        {
            get => setting;
            set => setting = value;
        }

        public static bool TryFind(SceneAsset sceneAsset, out UnityBakingSettingAsset settingAsset)
        {
            var assets = AssetDatabase.FindAssets($"t:{typeof(UnityBakingSettingAsset)}");
            foreach (var asset in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(asset);
                var settings = AssetDatabase.LoadAssetAtPath<UnityBakingSettingAsset>(path);
                if (settings.Setting.SceneAsset == sceneAsset)
                {
                    settingAsset = settings;
                    return true;
                }
            }

            settingAsset = default;
            return false;
        }

        public static UnityBakingSettingAsset FindOrCreate(SceneAsset sceneAsset)
        {
            if (TryFind(sceneAsset, out var settingAsset))
            {
                return settingAsset;
            }

            settingAsset = CreateInstance<UnityBakingSettingAsset>();
            var setting = new UnityBakingSetting(sceneAsset, TextureResizeType.One);
            settingAsset.setting = setting;
            var dirPath = setting.OutputAssetDirPath;
            AssetUtility.CreateFolderIfNecessary(dirPath);
            AssetDatabase.CreateAsset(settingAsset, Path.Combine(dirPath, "UnityBakingSettingAsset.asset"));
            AssetDatabase.Refresh();
            return settingAsset;
        }
    }
}
