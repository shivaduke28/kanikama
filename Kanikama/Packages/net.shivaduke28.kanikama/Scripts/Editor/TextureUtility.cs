using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Kanikama.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Kanikama.Editor
{
    public static class TextureUtility
    {
        class PackShaderProperties
        {
            public static readonly int Tex0 = Shader.PropertyToID("_Tex0");
            public static readonly int Tex1 = Shader.PropertyToID("_Tex1");
            public static readonly int Tex2 = Shader.PropertyToID("_Tex2");
            public static readonly int Tex3 = Shader.PropertyToID("_Tex3");
        }

        class RatioPackShaderProperties
        {
            public static readonly int Numerator0 = Shader.PropertyToID("_Numerator0");
            public static readonly int Numerator1 = Shader.PropertyToID("_Numerator1");
            public static readonly int Numerator2 = Shader.PropertyToID("_Numerator2");
            public static readonly int Numerator3 = Shader.PropertyToID("_Numerator3");
            public static readonly int Denominator0 = Shader.PropertyToID("_Denominator0");
            public static readonly int Denominator1 = Shader.PropertyToID("_Denominator1");
            public static readonly int Denominator2 = Shader.PropertyToID("_Denominator2");
            public static readonly int Denominator3 = Shader.PropertyToID("_Denominator3");
        }


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

        public static bool GetTextureHasMipmap(Texture2D texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return false;
            var textureImporter = (TextureImporter) AssetImporter.GetAtPath(path);
            return textureImporter.mipmapEnabled;
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

            var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dirPath, $"{name}.{ext}"));
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

            Debug.LogFormat(KanikamaDebug.Format, $"{path} has been saved.");
        }

        public static Texture2D GenerateTexture(TextureParameter textureParameter)
        {
            return new Texture2D(textureParameter.Width, textureParameter.Height, textureParameter.Format, textureParameter.MipChain, textureParameter.Linear);
        }

        public static RenderTexture GenerateRenderTexture(string dirPath, string name, RenderTextureDescriptor parameter)
        {
            var rt = new RenderTexture(parameter);
            var path = Path.Combine(dirPath, $"{name}.renderTexture");
            IOUtility.CreateOrReplaceAsset(ref rt, path);
            return rt;
        }

        public static Texture2D PackBC6H(Texture2D[] textures, bool isLinear)
        {
            var count = textures.Length;
            if (count == 0)
            {
                throw new ArgumentException(string.Format(KanikamaDebug.Format, "List is empty."));
            }
            if (count > 3)
            {
                throw new ArgumentException(string.Format(KanikamaDebug.Format, "The size of list must be <= 3."));
            }
            var shader = Shader.Find("Kanikama/Pack");
            var mat = new Material(shader);
            var map = textures[0];
            var rtDescriptor = new RenderTextureDescriptor(map.width, map.height)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                colorFormat = RenderTextureFormat.ARGBHalf,
                useMipMap = true,
                depthBufferBits = 0,
                useDynamicScale = false,
                msaaSamples = 1,
                volumeDepth = 1
            };
            var rt = RenderTexture.GetTemporary(rtDescriptor);

            var active = RenderTexture.active;
            var param = new TextureParameter
            {
                Width = map.width,
                Height = map.height,
                Format = TextureFormat.RGBAHalf, // map.format should be BC6H
                Linear = isLinear,
                MipChain = true,
            };

            Texture GetTexture(int index)
            {
                return index < count ? textures[index] : Texture2D.blackTexture;
            }

            mat.SetTexture(PackShaderProperties.Tex0, GetTexture(0));
            mat.SetTexture(PackShaderProperties.Tex1, GetTexture(1));
            mat.SetTexture(PackShaderProperties.Tex2, GetTexture(2));

            var tex = GenerateTexture(param);
            RenderTexture.active = rt;
            Graphics.Blit(null, rt, mat);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, true);
            tex.Apply();
            EditorUtility.CompressTexture(tex, TextureFormat.BC6H, TextureCompressionQuality.Best);
            RenderTexture.active = active;

            return tex;
        }

        public static Texture2D RatioPackBC6H((Texture2D Numerator, Texture2D Denominator)[] textures, bool isLinear)
        {
            var count = textures.Length;
            if (count == 0)
            {
                throw new ArgumentException(string.Format(KanikamaDebug.Format, "List is empty."));
            }
            if (count > 3)
            {
                throw new ArgumentException(string.Format(KanikamaDebug.Format, "The size of list must be <= 3."));
            }
            var shader = Shader.Find("Kanikama/RatioPack");
            var mat = new Material(shader);
            var map = textures[0].Numerator;
            var rtDescriptor = new RenderTextureDescriptor(map.width, map.height)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                colorFormat = RenderTextureFormat.ARGBHalf,
                useMipMap = true,
                depthBufferBits = 0,
                useDynamicScale = false,
                msaaSamples = 1,
                volumeDepth = 1
            };
            var rt = RenderTexture.GetTemporary(rtDescriptor);

            var active = RenderTexture.active;
            var param = new TextureParameter
            {
                Width = map.width,
                Height = map.height,
                Format = TextureFormat.RGBAHalf, // map.format should be BC6H
                Linear = isLinear,
                MipChain = true,
            };

            (Texture Numerator, Texture Denominator) GetTexture(int index)
            {
                return index < count ? textures[index] : (Texture2D.blackTexture, Texture2D.whiteTexture);
            }

            var (n0, d0) = GetTexture(0);
            var (n1, d1) = GetTexture(1);
            var (n2, d2) = GetTexture(2);
            mat.SetTexture(RatioPackShaderProperties.Numerator0, n0);
            mat.SetTexture(RatioPackShaderProperties.Numerator1, n1);
            mat.SetTexture(RatioPackShaderProperties.Numerator2, n2);
            mat.SetTexture(RatioPackShaderProperties.Denominator0, d0);
            mat.SetTexture(RatioPackShaderProperties.Denominator1, d1);
            mat.SetTexture(RatioPackShaderProperties.Denominator2, d2);

            var tex = GenerateTexture(param);
            RenderTexture.active = rt;
            Graphics.Blit(null, rt, mat);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, true);
            tex.Apply();
            EditorUtility.CompressTexture(tex, TextureFormat.BC6H, TextureCompressionQuality.Best);
            RenderTexture.active = active;

            return tex;
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
