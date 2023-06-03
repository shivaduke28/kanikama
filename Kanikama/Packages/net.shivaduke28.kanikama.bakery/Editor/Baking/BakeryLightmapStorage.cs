﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.Bakery.Editor.Baking
{
    [Serializable]
    public sealed class BakeryLightmapStorage
    {
        [Serializable]
        sealed class KeyLightmapsPair
        {
            public string Name;
            public string Key;
            public List<BakeryLightmap> Lightmaps;
        }

        [SerializeField] List<KeyLightmapsPair> lightmapsPairs = new List<KeyLightmapsPair>();
        public List<BakeryLightmap> Get() => lightmapsPairs.SelectMany(x => x.Lightmaps).ToList();

        public void AddOrUpdate(string key, List<BakeryLightmap> lightmaps, string name = "")
        {
            foreach (var pair in lightmapsPairs)
            {
                if (pair.Key == key)
                {
                    pair.Name = name;
                    pair.Lightmaps = lightmaps;
                    return;
                }
            }

            lightmapsPairs.Add(new KeyLightmapsPair
            {
                Key = key, Lightmaps = lightmaps, Name = name,
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

        public bool TryGet(string key, out List<BakeryLightmap> lightmaps)
        {
            foreach (var pair in lightmapsPairs)
            {
                if (pair.Key == key)
                {
                    lightmaps = pair.Lightmaps;
                    return true;
                }
            }

            lightmaps = default;
            return false;
        }

        public void Clear()
        {
            lightmapsPairs.Clear();
        }
    }
}