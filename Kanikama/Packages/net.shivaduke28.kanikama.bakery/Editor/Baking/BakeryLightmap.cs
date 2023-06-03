﻿using System;
using UnityEngine;

namespace Kanikama.Bakery.Editor.Baking
{
    [Serializable]
    public sealed class BakeryLightmap
    {
        [SerializeField] public BakeryLightmapType Type;
        [SerializeField] public Texture2D Texture;
        [SerializeField] public string Path;
        [SerializeField] public int Index;

        public BakeryLightmap(BakeryLightmapType type, Texture2D texture, string path, int index)
        {
            Type = type;
            Texture = texture;
            Path = path;
            Index = index;
        }
    }

    public enum BakeryLightmapType
    {
        Light = 0,
        Directional = 1,
        L0 = 2,
        L1 = 3,
    }

    public static class BakeryLightmapTypeExtension
    {
        public static string ToFileName(this BakeryLightmapType type)
        {
            switch (type)
            {
                case BakeryLightmapType.Light:
                    return "light";
                case BakeryLightmapType.Directional:
                    return "dir";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}