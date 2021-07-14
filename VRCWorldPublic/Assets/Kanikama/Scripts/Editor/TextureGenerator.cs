using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;


public class TextureGenerator
{
    [MenuItem("UdonGI/GenerateTexture")]
    public static void GenerateTexture()
    {
        var texture = new Texture2D(256, 256, TextureFormat.RGBA32, false, linear: true);
        var bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes("Assets/texture.png", bytes);
        AssetDatabase.Refresh();
    }
}
