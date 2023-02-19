using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Core.Editor
{
    [CreateAssetMenu(menuName = "Kanikama/BakedAssetRegistry", fileName = "BakedAssetRegistry")]
    public sealed class BakedAssetRegistry : ScriptableObject
    {
        public const string DefaultFileName = "BakeAssetRegistry.asset";
        [SerializeField] List<BakedAssetKeyValuePair> map = new List<BakedAssetKeyValuePair>();

        public BakedAssetData[] GetAllBakedAssetDatum() => map.Select(kvp => kvp.BakedAssetData).ToArray();

        public bool TryGet(string key, out BakedAssetData data)
        {
            foreach (var pair in map)
            {
                if (pair.Key == key)
                {
                    data = pair.BakedAssetData;
                    return true;
                }
            }
            data = default;
            return false;
        }

        public void AddOrUpdate(string key, BakedAssetData data)
        {
            foreach (var pair in map)
            {
                if (pair.Key == key)
                {
                    pair.BakedAssetData = data;
                    return;
                }
            }

            map.Add(new BakedAssetKeyValuePair
            {
                Key = key, BakedAssetData = data,
            });
        }

        public void Remove(string key)
        {
            var index = map.FindIndex(pair => pair.Key == key);
            map.RemoveAt(index);
        }

        public void Clear()
        {
            map.Clear();
        }


        [Serializable]
        public sealed class BakedAssetKeyValuePair
        {
            public string Key;
            public BakedAssetData BakedAssetData;
        }


        public static BakedAssetRegistry FindOrCreate(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<BakedAssetRegistry>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = CreateInstance<BakedAssetRegistry>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
