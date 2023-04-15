using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kanikama.Core.Editor;
using Kanikama.Core.Editor.Textures;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Bakery.Editor
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
            setting.Set(sceneAsset);
            settingAsset.setting = setting;
            var dirPath = setting.OutputAssetDirPath;
            KanikamaSceneUtility.CreateFolderIfNecessary(dirPath);
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
        [SerializeField] List<KeyLightmapsPair> lightmapsPairs = new List<KeyLightmapsPair>();
        [SerializeField] string outputAssetDirPath;

        [Serializable]
        sealed class KeyLightmapsPair
        {
            public string Key;
            public List<BakeryLightmap> Lightmaps;
        }


        public SceneAsset SceneAsset => sceneAsset;
        public List<BakeryLightmap> GetBakeryLightmaps() => lightmapsPairs.SelectMany(pair => pair.Lightmaps).ToList();
        public TextureResizeType TextureResizeType => textureResizeType;
        public string OutputAssetDirPath => outputAssetDirPath;

        public void AddOrUpdate(string key, List<BakeryLightmap> lightmaps)
        {
            foreach (var pair in lightmapsPairs)
            {
                if (pair.Key == key)
                {
                    pair.Lightmaps = lightmaps;
                    return;
                }
            }

            lightmapsPairs.Add(new KeyLightmapsPair
            {
                Key = key, Lightmaps = lightmaps,
            });
        }

        public void Remove(string key)
        {
            var index = lightmapsPairs.FindIndex(pair => pair.Key == key);
            if (index >= 0)
            {
                lightmapsPairs.RemoveAt(index);
            }
        }

        public void Set(SceneAsset scene)
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
