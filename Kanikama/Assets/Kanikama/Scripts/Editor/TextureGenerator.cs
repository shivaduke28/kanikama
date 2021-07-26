using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Kanikama.Editor
{
    public class TextureGenerator
    {
        public static Texture2D GenerateTexture(string path, Parameter parameter)
        {
            return GenerateTexture(path, parameter.width, parameter.height, parameter.format, parameter.mipChain, parameter.linear);
        }

        public static Texture2D GenerateTexture(string path, int width, int height, TextureFormat format, bool mipChain, bool linear)
        {
            var texture = new Texture2D(width, height, format, mipChain, linear);
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            Debug.Log($"{path} has been generated.");
            return texture;
        }

        public class Parameter
        {
            public int width = 256;
            public int height = 256;
            public TextureFormat format = TextureFormat.RGBA32;
            public bool mipChain = true;
            public bool linear = true;
        }
    }
}
