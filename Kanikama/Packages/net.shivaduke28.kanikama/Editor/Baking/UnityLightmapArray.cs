using System;
using System.Collections.Generic;
using Kanikama.Core.Editor;
using UnityEngine;

namespace Kanikama.Baking.Editor
{
    [Serializable]
    public sealed class UnityLightmapArray
    {
        [SerializeField] public UnityLightmapType Type;
        [SerializeField] public Texture2DArray Texture;
        [SerializeField] public string Path;
        [SerializeField] public int Index;

        public UnityLightmapArray(UnityLightmapType type, Texture2DArray texture, string path, int index)
        {
            Type = type;
            Texture = texture;
            Path = path;
            Index = index;
        }
    }

    [Serializable]
    public sealed class UnityLightmapArrayStorage
    {
        [SerializeField] List<UnityLightmapArray> lightmapArrays = new List<UnityLightmapArray>();
        public IReadOnlyList<UnityLightmapArray> LightmapArrays => lightmapArrays;

        public void Clear()
        {
            lightmapArrays.Clear();
        }

        public void AddOrUpdate(UnityLightmapArray lightmapArray)
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
