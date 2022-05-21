using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.Baking
{
    [Serializable]
    public class BakedAsset
    {
        public List<Texture2DArray> kanikamaMapArrays = new List<Texture2DArray>();
        public List<Texture2DArray> kanikamaDirectionalMapArrays = new List<Texture2DArray>();
        public List<CustomRenderTexture> customRenderTextures = new List<CustomRenderTexture>();
        public List<Material> customRenderTextureMaterials = new List<Material>();
        public List<RenderTexture> renderTextures = new List<RenderTexture>();
        public List<Material> renderTextureMaterials = new List<Material>();
        public int sliceCount;

        public void RemoveNullAssets()
        {
            kanikamaMapArrays.RemoveAll(x => x == null);
            kanikamaDirectionalMapArrays.RemoveAll(x => x == null);
            customRenderTextures.RemoveAll(x => x == null);
            customRenderTextureMaterials.RemoveAll(x => x == null);
            renderTextures.RemoveAll(x => x == null);
            renderTextureMaterials.RemoveAll(x => x == null);
        }
    }
}