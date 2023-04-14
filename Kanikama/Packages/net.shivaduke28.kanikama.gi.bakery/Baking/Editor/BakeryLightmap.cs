using System;
using UnityEngine;

namespace Kanikama.GI.Bakery.Editor
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
        Color = 0,
        Directional = 1,
        // TODO: MonoSH = 2,
    }
}
