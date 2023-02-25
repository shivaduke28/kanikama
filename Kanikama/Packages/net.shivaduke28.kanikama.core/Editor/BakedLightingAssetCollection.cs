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
    public sealed class BakedLightingAssetCollection
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

    public static class LightmapTypeExtension
    {
        public static string ToFileName(this LightmapType lightmapType)
        {
            switch (lightmapType)
            {
                case LightmapType.Color:
                    return "light";
                case LightmapType.Directional:
                    return "dir";
                case LightmapType.ShadowMask:
                    return "shadowmask";
                default:
                    throw new ArgumentOutOfRangeException(nameof(lightmapType), lightmapType, null);
            }
        }
    }
}
