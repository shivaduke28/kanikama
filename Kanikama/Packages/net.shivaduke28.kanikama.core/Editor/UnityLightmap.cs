using System;
using UnityEngine;

namespace Kanikama.Core.Editor
{
    [Serializable]
    public sealed class UnityLightmap
    {
        [SerializeField] public UnityLightmapType Type;
        [SerializeField] public Texture2D Texture;
        [SerializeField] public string Path;
        [SerializeField] public int Index;

        public UnityLightmap(UnityLightmapType type, Texture2D texture, string path, int index)
        {
            Type = type;
            Texture = texture;
            Path = path;
            Index = index;
        }
    }

    public enum UnityLightmapType
    {
        Color = 0,
        Directional = 1,
        ShadowMask = 2,
    }

    public static class LightmapTypeExtension
    {
        public static string ToFileName(this UnityLightmapType unityLightmapType)
        {
            switch (unityLightmapType)
            {
                case UnityLightmapType.Color:
                    return "light";
                case UnityLightmapType.Directional:
                    return "dir";
                case UnityLightmapType.ShadowMask:
                    return "shadowmask";
                default:
                    throw new ArgumentOutOfRangeException(nameof(unityLightmapType), unityLightmapType, null);
            }
        }
    }
}
