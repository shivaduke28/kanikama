using System;
using Kanikama.Core.Editor.Textures;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Editor
{
    [Serializable]
    public sealed class BakingConfiguration
    {
        [SerializeField] SceneAsset sceneAsset;
        [SerializeField] TextureResizeType textureResizeType;

        public SceneAsset SceneAsset => sceneAsset;
        public TextureResizeType TextureResizeType => textureResizeType;


        public BakingConfiguration(SceneAsset sceneAsset,
            TextureResizeType textureResizeType)
        {
            this.sceneAsset = sceneAsset;
            this.textureResizeType = textureResizeType;
        }

        public BakingConfiguration Clone()
        {
            return new BakingConfiguration(sceneAsset, textureResizeType);
        }
    }
}
