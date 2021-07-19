using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

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

        /// <summary>
        /// Texture2DArrayを作成する
        /// </summary>
        /// <param name="textures">元となるTexture2Dの配列</param>
        /// <param name="isLinear">色空間がリニアかどうか（UnityでベイクしたLightmapはsRGBなのでfalse）</param>
        /// <returns></returns>
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