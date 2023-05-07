using System;
using System.Text.RegularExpressions;
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

        public static readonly Regex FileNameRegex = new Regex("Lightmap-[0-9]+_comp_[light|dir|shadowmask]");

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
}
