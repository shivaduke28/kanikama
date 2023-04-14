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
        [SerializeField] string lightmapperKey;

        public SceneAsset SceneAsset => sceneAsset;
        public TextureResizeType TextureResizeType => textureResizeType;
        public string LightmapperKey => lightmapperKey;


        public BakingConfiguration(SceneAsset sceneAsset,
            TextureResizeType textureResizeType,
            string lightmapperKey)
        {
            this.sceneAsset = sceneAsset;
            this.textureResizeType = textureResizeType;
            this.lightmapperKey = lightmapperKey;
        }

        public BakingConfiguration Clone()
        {
            return new BakingConfiguration(sceneAsset, textureResizeType, lightmapperKey);
        }
    }
}
