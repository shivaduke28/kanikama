using System;
using System.IO;
using Kanikama.Editor;
using Kanikama.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Bakery.Editor
{
    [CreateAssetMenu(menuName = "Kanikama/BakeryBakingSettingAsset", fileName = "BakeryBakingSettingAsset")]
    public sealed class BakeryBakingSettingAsset : ScriptableObject
    {
        [SerializeField] BakeryBakingSetting setting;

        public BakeryBakingSetting Setting
        {
            get => setting;
            set => setting = value;
        }

        public static bool TryFind(SceneAsset sceneAsset, out BakeryBakingSettingAsset settingAsset)
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(BakeryBakingSettingAsset)}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<BakeryBakingSettingAsset>(path);
                if (asset.Setting.SceneAsset == sceneAsset)
                {
                    settingAsset = asset;
                    return true;
                }
            }

            settingAsset = default;
            return false;
        }

        public static BakeryBakingSettingAsset FindOrCreate(SceneAsset sceneAsset)
        {
            if (TryFind(sceneAsset, out var settingAsset))
            {
                return settingAsset;
            }

            settingAsset = CreateInstance<BakeryBakingSettingAsset>();
            var setting = new BakeryBakingSetting();
            setting.SetSceneAsset(sceneAsset);
            settingAsset.setting = setting;
            var dirPath = setting.OutputAssetDirPath;
            AssetUtility.CreateFolderIfNecessary(dirPath);
            AssetDatabase.CreateAsset(settingAsset, Path.Combine(dirPath, "BakeryBakingSettingAsset.asset"));
            AssetDatabase.Refresh();
            return settingAsset;
        }
    }

    [Serializable]
    public class BakeryBakingSetting
    {
        [SerializeField] SceneAsset sceneAsset;
        [SerializeField] TextureResizeType textureResizeType = TextureResizeType.One;
        [SerializeField] AssetStorage assetStorage = new AssetStorage();
        [SerializeField] string outputAssetDirPath = "Assets";

        public SceneAsset SceneAsset => sceneAsset;
        public AssetStorage AssetStorage => assetStorage;
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
