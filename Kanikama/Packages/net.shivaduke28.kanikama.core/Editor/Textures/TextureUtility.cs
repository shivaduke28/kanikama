using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Kanikama.Core.Editor.Textures
{
    public static class TextureUtility
    {
        // isLinear:
        // - lightmap: false
        // - directional lightmap: true
        // - shadow mask: ?
        public static Texture2DArray CreateTexture2DArray(List<Texture2D> textures, bool isLinear, bool mipChain)
        {
            var count = textures.Count;
            if (count == 0)
            {
                throw new ArgumentException("textures is empty.");
            }

            var map = textures[0];
            var texArray = new Texture2DArray(map.width, map.height, count, map.format, mipChain, isLinear)
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

        public static void ResizeTexture(Texture2D texture, TextureResizeType textureResizeType)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return;
            var textureImporter = (TextureImporter) AssetImporter.GetAtPath(path);
            GetTextureRealWidthAndHeight(textureImporter, out var width, out _);
            switch (textureResizeType)
            {
                case TextureResizeType.One:
                    break;
                case TextureResizeType.OneHalf:
                    width /= 2;
                    break;
                case TextureResizeType.OneQuarter:
                    width /= 4;
                    break;
                case TextureResizeType.OneEighth:
                    width /= 8;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(textureResizeType), textureResizeType, null);
            }
            width = Mathf.Max(1, width);
            textureImporter.maxTextureSize = width;
            textureImporter.SaveAndReimport();
        }

        // https://answers.unity.com/questions/893447/get-the-real-texture-size.html
        public static void GetTextureRealWidthAndHeight(TextureImporter textureImporter, out int width, out int height)
        {
            width = 0;
            height = 0;
            var type = typeof(TextureImporter);
            var method = type.GetMethod("GetWidthAndHeight", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsTrue(method != null);
            var args = new object[] { width, height };
            method.Invoke(textureImporter, args);
            width = (int) args[0];
            height = (int) args[1];
        }


        public static void SaveTexture2D(Texture2D texture, string dirPath, string name, TextureImportParameter parameter)
        {
            string ext;
            byte[] bytes;
            if (parameter.Extension == TextureExtension.Png)
            {
                ext = "png";
                bytes = texture.EncodeToPNG();
            }
            else
            {
                ext = "exr";
                bytes = texture.EncodeToEXR();
            }

            var path = Path.Combine(dirPath, $"{name}.{ext}");
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            var textureImporter = (TextureImporter) AssetImporter.GetAtPath(path);
            textureImporter.isReadable = parameter.IsReadable;
            textureImporter.textureCompression = parameter.Compression;
            textureImporter.SaveAndReimport();
            if (parameter.Compression != TextureImporterCompression.Uncompressed)
            {
                EditorUtility.CompressTexture(texture, parameter.CompressedFormat, parameter.CompressionQuality);
            }

            KanikamaDebug.Log($"{path} has been saved.");
        }

        public static Texture2D GenerateTexture(TextureParameter textureParameter)
        {
            return new Texture2D(textureParameter.Width, textureParameter.Height, textureParameter.Format, textureParameter.MipChain, textureParameter.Linear);
        }

        public static RenderTexture GenerateRenderTexture(string dirPath, string name, RenderTextureDescriptor parameter)
        {
            var rt = new RenderTexture(parameter);
            var path = Path.Combine(dirPath, $"{name}.renderTexture");
            KanikamaSceneUtility.CreateOrReplaceAsset(ref rt, path);
            return rt;
        }
    }

    [Serializable]
    public class TextureParameter
    {
        public int Width = 256;
        public int Height = 256;
        public TextureFormat Format = TextureFormat.RGBAHalf;
        public bool MipChain = true;
        public bool Linear = true;
    }

    [Serializable]
    public class TextureImportParameter
    {
        public TextureExtension Extension = TextureExtension.Exr;
        public bool IsReadable = true;
        public TextureImporterCompression Compression = TextureImporterCompression.Uncompressed;
        public TextureFormat CompressedFormat = TextureFormat.BC6H;
        public TextureCompressionQuality CompressionQuality = TextureCompressionQuality.Best;
    }

    public enum TextureExtension
    {
        Png = 0,
        Exr = 1,
    }

    public enum TextureResizeType
    {
        One = 1,
        OneHalf = 2,
        OneQuarter = 4,
        OneEighth = 8,
    }
}
