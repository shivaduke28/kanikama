using System.IO;
using Kanikama.Core.Editor.Util;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Bakery.Baking.Editor
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
            IOUtility.CreateFolderIfNecessary(dirPath);
            AssetDatabase.CreateAsset(settingAsset, Path.Combine(dirPath, "BakeryBakingSettingAsset.asset"));
            AssetDatabase.Refresh();
            return settingAsset;
        }
    }
}
