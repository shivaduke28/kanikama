using System;
using Kanikama.Core.Editor;
using Kanikama.Core.Editor.Textures;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Editor
{
    [Serializable]
    public sealed class UnityBakingSetting
    {
        [SerializeField] SceneAsset sceneAsset;
        [SerializeField] TextureResizeType textureResizeType = TextureResizeType.One;
        [SerializeField] UnityLightmapStorage lightmapStorage = null;

        public SceneAsset SceneAsset
        {
            get => sceneAsset;
            set => sceneAsset = value;
        }

        public TextureResizeType TextureResizeType => textureResizeType;
        public UnityLightmapStorage LightmapStorage => lightmapStorage;

        public UnityBakingSetting(SceneAsset sceneAsset, TextureResizeType textureResizeType)
        {
            this.sceneAsset = sceneAsset;
            this.textureResizeType = textureResizeType;
        }

        public UnityBakingSetting Clone()
        {
            return new UnityBakingSetting(sceneAsset, textureResizeType);
        }
    }
}
