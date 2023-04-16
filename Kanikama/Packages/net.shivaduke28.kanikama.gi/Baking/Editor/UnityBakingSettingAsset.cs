﻿using System.IO;
using Kanikama.Core.Editor;
using Kanikama.Core.Editor.Textures;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Editor
{
    [CreateAssetMenu(menuName = "Kanikama/GI/BakingConfiguration", fileName = "KanikamaGIBakingConfiguration")]
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
            KanikamaSceneUtility.CreateFolderIfNecessary(dirPath);
            AssetDatabase.CreateAsset(settingAsset, Path.Combine(dirPath, "BakeryBakingSettingAsset.asset"));
            AssetDatabase.Refresh();
            return settingAsset;
        }
    }
}