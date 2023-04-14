using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.Core.Editor
{
    [Serializable]
    public sealed class BakedAssetDataBase
    {
        [SerializeField] List<BakedAssetKeyValuePair> map = new List<BakedAssetKeyValuePair>();
        public BakedLightingAssetCollection[] GetAllBakedAssets() => map.Select(kvp => kvp.bakedLightingAssetCollection).ToArray();

        public bool TryGet(string key, out BakedLightingAssetCollection data)
        {
            foreach (var pair in map)
            {
                if (pair.Key == key)
                {
                    data = pair.bakedLightingAssetCollection;
                    return true;
                }
            }

            data = default;
            return false;
        }

        public void AddOrUpdate(string key, BakedLightingAssetCollection data)
        {
            foreach (var pair in map)
            {
                if (pair.Key == key)
                {
                    pair.bakedLightingAssetCollection = data;
                    return;
                }
            }

            map.Add(new BakedAssetKeyValuePair
            {
                Key = key, bakedLightingAssetCollection = data,
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
            public BakedLightingAssetCollection bakedLightingAssetCollection;
        }
    }
}
