using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Core.Editor
{
    public class KanikamaTextureUtility
    {
        // isLinear:
        // - lightmap: false
        // - directional lightmap: true
        // - shadow mask: ?
        public static Texture2DArray CreateTexture2DArray(List<Texture2D> textures, bool isLinear)
        {
            var count = textures.Count;
            if (count == 0)
            {
                throw new ArgumentException("textures is empty.");
            }

            var map = textures[0];
            var texArray = new Texture2DArray(map.width, map.height, count, map.format, true, isLinear)
            {
                anisoLevel = map.anisoLevel,
                wrapMode = map.wrapMode,
                filterMode = map.filterMode,
            };
            for (var i = 0; i < count; i++)
            {
                var lightmap = textures[i];
                Graphics.CopyTexture(lightmap, 0, texArray, i);
            }

            return texArray;
        }

        // supported format: RenderTextureFormat.ARGBHalf
        // isLinear:
        // - lightmap: false
        public static Texture2D CompressToBC6H(RenderTexture renderTexture, bool isLinear, bool useMipmap = true,
            TextureCompressionQuality quality = TextureCompressionQuality.Best)
        {
            var texture = new Texture2D(
                renderTexture.width,
                renderTexture.height,
                TextureFormat.RGBAHalf,
                useMipmap,
                isLinear);
            var active = RenderTexture.active;
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, useMipmap);
            EditorUtility.CompressTexture(texture, TextureFormat.BC6H, quality);
            RenderTexture.active = active;
            return texture;
        }

        // create a RenderTexture with RGB = max(0, texture0.rgb - texture1.rgb)
        public static RenderTexture Subtract(Texture texture0, Texture texture1, int width, int height)
        {
            var desc = new RenderTextureDescriptor(width, height)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                colorFormat = RenderTextureFormat.ARGBHalf,
                useMipMap = true,
                depthBufferBits = 0,
                useDynamicScale = false,
                msaaSamples = 1,
                volumeDepth = 1
            };

            var active = RenderTexture.active;
            var renderTexture = new RenderTexture(desc);

            var shader = Shader.Find("Kanikama/Subtract");
            var mat = new Material(shader);

            mat.SetTexture("_Tex0", texture0);
            mat.SetTexture("_Tex1", texture1);
            RenderTexture.active = renderTexture;
            Graphics.Blit(null, renderTexture, mat);
            RenderTexture.active = active;
            UnityEngine.Object.DestroyImmediate(mat);
            return renderTexture;
        }
    }
}
