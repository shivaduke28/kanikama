using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Kanikama.Baking
{
    public class TextureGenerator
    {
        public static Texture2D GenerateTexture(string dirPath, string name, Parameter parameter)
        {
            var texture = GenerateTexture(parameter);
            var ext = parameter.extension == TextureExtension.Png ? "png" : "exr";
            var path = Path.Combine(dirPath, $"{name}.{ext}");
            var bytes = texture.EncodeToEXR();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.isReadable = parameter.isReadable;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.SaveAndReimport();
            Debug.Log($"{path} has been generated.");
            return texture;
        }

        public static Texture2D GenerateTexture(Parameter parameter)
        {
            return new Texture2D(parameter.width, parameter.height, parameter.format, parameter.mipChain, parameter.linear);
        }

        public static RenderTexture GenerateRenderTexture(string dirPath, string name, RenderTextureDescriptor parameter)
        {
            var rt = new RenderTexture(parameter);
            var path = Path.Combine(dirPath, $"{name}.renderTexture");
            KanikamaEditorUtility.CreateOrReplaceAsset<RenderTexture>(ref rt, path);
            return rt;
        }

        public class Parameter
        {
            public int width = 256;
            public int height = 256;
            public TextureFormat format = TextureFormat.RGBAHalf;
            public bool mipChain = true;
            public bool linear = true;
            public TextureExtension extension = TextureExtension.Exr;
            public bool isReadable = true;
        }

        public enum TextureExtension
        {
            Png = 0,
            Exr = 1,
        }
    }
}
