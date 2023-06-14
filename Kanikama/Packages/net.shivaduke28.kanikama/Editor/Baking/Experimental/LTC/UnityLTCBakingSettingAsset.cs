using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.Baking.Experimental.LTC
{
    [CreateAssetMenu(menuName = "Kanikama/UnityLTCBakingSettingAsset", fileName = "UnityLTCBakingSettingAsset")]
    public class UnityLTCBakingSettingAsset : ScriptableObject
    {
        [SerializeField] UnityBakingSetting setting;

        public UnityBakingSetting Setting
        {
            get => setting;
            set => setting = value;
        }

        public static bool TryFind(SceneAsset sceneAsset, out UnityLTCBakingSettingAsset settingAsset)
        {
            var assets = AssetDatabase.FindAssets($"t:{typeof(UnityLTCBakingSettingAsset)}");
            foreach (var asset in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(asset);
                var settings = AssetDatabase.LoadAssetAtPath<UnityLTCBakingSettingAsset>(path);
                if (settings.Setting.SceneAsset == sceneAsset)
                {
                    settingAsset = settings;
                    return true;
                }
            }

            settingAsset = default;
            return false;
        }

        public static UnityLTCBakingSettingAsset FindOrCreate(SceneAsset sceneAsset)
        {
            if (TryFind(sceneAsset, out var settingAsset))
            {
                return settingAsset;
            }

            settingAsset = CreateInstance<UnityLTCBakingSettingAsset>();
            var setting = new UnityBakingSetting(sceneAsset, TextureResizeType.One, "_kanikama_unity_ltc");
            settingAsset.setting = setting;
            var dirPath = setting.OutputAssetDirPath;
            IOUtility.CreateFolderIfNecessary(dirPath);
            AssetDatabase.CreateAsset(settingAsset, Path.Combine(dirPath, "UnityLTCBakingSettingAsset.asset"));
            AssetDatabase.Refresh();
            return settingAsset;
        }
    }
}
