using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Kanikama.Editor
{
    public class TextureGenerator
    {
        [MenuItem("Kanikama/GenerateTexture256")]
        public static void GenerateTexture()
        {
            var texture = new Texture2D(256, 256, TextureFormat.RGBA32, false, linear: true);
            var bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes("Assets/texture.png", bytes);
            AssetDatabase.Refresh();
        }
    }
}
