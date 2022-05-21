using System;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Kanikama.Baking
{
    public class TextureGenerator
    {
        public static void SaveTexture2D(Texture2D texture, string dirPath, string name, ImportParameter parameter)
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

            Debug.Log($"{path} has been saved.");
        }

        public static Texture2D GenerateTexture(Parameter parameter)
        {
            return new Texture2D(parameter.Width, parameter.Height, parameter.Format, parameter.MipChain, parameter.Linear);
        }

        public static RenderTexture GenerateRenderTexture(string dirPath, string name, RenderTextureDescriptor parameter)
        {
            var rt = new RenderTexture(parameter);
            var path = Path.Combine(dirPath, $"{name}.renderTexture");
            KanikamaEditorUtility.CreateOrReplaceAsset(ref rt, path);
            return rt;
        }

        [Serializable]
        public class Parameter
        {
            public int Width = 256;
            public int Height = 256;
            public TextureFormat Format = TextureFormat.RGBAHalf;
            public bool MipChain = true;
            public bool Linear = true;
        }

        [Serializable]
        public class ImportParameter
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
    }
}