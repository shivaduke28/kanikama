using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor
{
    [CreateAssetMenu(fileName = "texture2d_array_converter", menuName = "Kanikama/Texture2DArrayGenerator")]
    public class Texture2DArrayGenerator : ScriptableObject
    {
        public List<Texture2D> textures = new List<Texture2D>();
        public string fileName = "texture2d_array";

        [ContextMenu("Generate")]
        public Texture2DArray Generate()
        {
            var texArray = Generate(textures);
            var path = AssetDatabase.GetAssetPath(this);
            var dir = Path.GetDirectoryName(path);
            AssetUtil.CreateOrReplaceAsset(ref texArray, Path.Combine(dir, $"{fileName}.asset"));
            AssetDatabase.Refresh();
            return texArray;
        }

        [ContextMenu("Generate (Linear)")]
        public Texture2DArray GenerateLinear()
        {
            var texArray = Generate(textures, true);
            var path = AssetDatabase.GetAssetPath(this);
            var dir = Path.GetDirectoryName(path);
            AssetUtil.CreateOrReplaceAsset(ref texArray, Path.Combine(dir, $"{fileName}.asset"));
            AssetDatabase.Refresh();
            return texArray;
        }

        public static Texture2DArray Generate(List<Texture2D> textures, bool isLinear = false)
        {
            var count = textures.Count;
            if (count == 0)
            {
                throw new System.Exception("テクスチャの配列が空です");
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
                var lightMap = textures[i];
                Graphics.CopyTexture(lightMap, 0, texArray, i);
            }
            return texArray;
        }
    }
}