using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.GI.Bakery.Editor
{
    [Serializable]
    public sealed class BakeryLightmapArray
    {
        [SerializeField] public BakeryLightmapType Type;
        [SerializeField] public Texture2DArray Texture;
        [SerializeField] public string Path;
        [SerializeField] public int Index;

        public BakeryLightmapArray(BakeryLightmapType type, Texture2DArray texture, string path, int index)
        {
            Type = type;
            Texture = texture;
            Path = path;
            Index = index;
        }
    }

    [Serializable]
    public sealed class BakeryLightmapArrayStorage
    {
        [SerializeField] List<BakeryLightmapArray> lightmapArrays = new List<BakeryLightmapArray>();

        public void Clear()
        {
            lightmapArrays.Clear();
        }

        public void AddOrUpdate(BakeryLightmapArray lightmapArray)
        {
            var index = lightmapArrays.FindIndex(l => l.Index == lightmapArray.Index && l.Type == lightmapArray.Type);
            if (index >= 0)
            {
                lightmapArrays[index] = lightmapArray;
            }
            else
            {
                lightmapArrays.Add(lightmapArray);
            }
        }
    }
}
