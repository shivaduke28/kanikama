using System;
using UnityEngine;

namespace Kanikama.Utility.Editor
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
        Light = 0,
        Directional = 1,
        ShadowMask = 2,
    }
}
