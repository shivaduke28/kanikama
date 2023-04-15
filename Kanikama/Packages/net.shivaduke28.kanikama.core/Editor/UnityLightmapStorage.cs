using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.Core.Editor
{
    [Serializable]
    public sealed class UnityLightmapStorage
    {
        [Serializable]
        sealed class KeyLightmapsPair
        {
            public string Key;
            public List<UnityLightmap> Lightmaps;
        }

        [SerializeField] List<KeyLightmapsPair> lightmapsPairs = new List<KeyLightmapsPair>();
        public List<UnityLightmap> Get() => lightmapsPairs.SelectMany(x => x.Lightmaps).ToList();

        public void AddOrUpdate(string key, List<UnityLightmap> lightmaps)
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

        public bool Remove(string key)
        {
            var index = lightmapsPairs.FindIndex(pair => pair.Key == key);
            if (index >= 0)
            {
                lightmapsPairs.RemoveAt(index);
                return true;
            }
            return false;
        }

        public bool TryGet(string key, out List<UnityLightmap> data)
        {
            foreach (var pair in lightmapsPairs)
            {
                if (pair.Key == key)
                {
                    data = pair.Lightmaps;
                    return true;
                }
            }

            data = default;
            return false;
        }

        public void Clear()
        {
            lightmapsPairs.Clear();
        }
    }
}
