using System;
using System.Collections.Generic;
using System.IO;
using Kanikama.Core.Editor;
using Kanikama.Core.Editor.Textures;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Bakery.Editor
{
    [CreateAssetMenu(menuName = "Kanikama/BakeryBakingSettingAsset", fileName = "BakeryBakingSettingAsset")]
    public sealed class BakeryBakingSettingAsset : ScriptableObject
    {
        [SerializeField] SceneAsset sceneAsset;
        [SerializeField] TextureResizeType textureResizeType = TextureResizeType.One;
        [SerializeField] List<KeyLightmapsPair> lightmapsPairs;

        [Serializable]
        sealed class KeyLightmapsPair
        {
            public string Key;
            public List<BakeryLightmap> Lightmaps;
        }

        public SceneAsset SceneAsset => sceneAsset;

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

        public static BakeryBakingSettingAsset Find(SceneAsset sceneAsset)
        {
            var assets = AssetDatabase.FindAssets($"t:{typeof(BakeryBakingSettingAsset)}");
            foreach (var asset in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(asset);
                var settings = AssetDatabase.LoadAssetAtPath<BakeryBakingSettingAsset>(path);
                if (settings.SceneAsset == sceneAsset) return settings;
            }

            return null;
        }

        public static BakeryBakingSettingAsset FindOrCreate(SceneAsset sceneAsset)
        {
            var setting = Find(sceneAsset);
            if (setting != null) return setting;
            setting = CreateInstance<BakeryBakingSettingAsset>();
            var sceneAssetData = KanikamaSceneUtility.ToAssetData(sceneAsset);

            var dirPath = sceneAssetData + "_kanikama_bakery"; // todo: 良い感じに...
            KanikamaSceneUtility.CreateFolderIfNecessary(dirPath);
            AssetDatabase.CreateAsset(setting, Path.Combine(dirPath, "BakeryBakingSettingAsset.asset"));
            AssetDatabase.Refresh();
            return setting;
        }
    }
}
