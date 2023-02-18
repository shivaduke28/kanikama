using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.Core.Editor
{
    [Serializable]
    public sealed class BakedLightmap
    {
        [SerializeField] public LightmapType Type;
        [SerializeField] public Texture2D Texture;
        [SerializeField] public string Path;
        [SerializeField] public int Index;

        public BakedLightmap(LightmapType type, Texture2D texture, string path, int index)
        {
            Type = type;
            Texture = texture;
            Path = path;
            Index = index;
        }
    }

    [Serializable]
    public sealed class BakedAssetData
    {
        [SerializeField] public List<BakedLightmap> Lightmaps = new List<BakedLightmap>();
        [SerializeField] public List<BakedLightmap> DirectionalLightmaps = new List<BakedLightmap>();
        [SerializeField] public List<BakedLightmap> ShadowMasks = new List<BakedLightmap>();
    }

    public enum LightmapType
    {
        Color = 0,
        Directional = 1,
        ShadowMask = 2,
    }
}
