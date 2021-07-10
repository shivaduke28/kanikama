using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
namespace Kanikama.Editor
{
    [CreateAssetMenu(fileName = "texture2d_array_converter", menuName = "Kanikama/Texture2DArrayConverter")]
    public class Texture2DArrayConverter : ScriptableObject
    {
        public List<Texture2D> textures = new List<Texture2D>();
        public string fileName = "texture2d_array";

        [ContextMenu("Convert")]
        public Texture2DArray Convert()
        {
            var count = textures.Count;
            if (count == 0)
            {
                throw new System.Exception("テクスチャをセットしてください");
            }

            var map = textures[0];
            var texArray = new Texture2DArray(map.width, map.height, count, map.format, true, true)
            {
                anisoLevel = map.anisoLevel,
                wrapMode = map.wrapMode,
                filterMode = map.filterMode,
            };
            for (var i = 0; i < count; i++)
            {
                var lightMap = textures[i];
                var color = lightMap.GetPixels();
                texArray.SetPixels(color, i);
            }
            texArray.Apply();

            var path = AssetDatabase.GetAssetPath(this);
            var dir = Path.GetDirectoryName(path);
            AssetUtil.CreateOrReplaceAsset(ref texArray, Path.Combine(dir, $"{fileName}.asset"));
            AssetDatabase.Refresh();
            return texArray;
        }
    }
}