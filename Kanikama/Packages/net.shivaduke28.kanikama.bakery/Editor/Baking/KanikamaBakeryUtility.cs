using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Kanikama.Editor.Baking;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Bakery.Editor.Baking
{
    public static class KanikamaBakeryUtility
    {
        // TODO: 名前良い感じに...
        static readonly Regex LM0 = new Regex(@"^LM0");
        static readonly Regex LMA = new Regex(@"^LMA[0-9]+");
        static readonly Regex Color = new Regex(@"final$");
        static readonly Regex Dir = new Regex(@"dir$");
        static readonly Regex L0 = new Regex(@"L0$");
        static readonly Regex L1 = new Regex(@"L1$");

        // "{scene name}_[LM0|LMA[0-9]+]_[final|dir|L0|L1]"
        public static List<Lightmap> GetLightmaps(string outputAssetDirPath, string sceneName)
        {
            var result = new List<Lightmap>();
            var regex = new Regex($"^{sceneName}_");
            foreach (var guid in AssetDatabase.FindAssets("t:Texture", new[] { outputAssetDirPath }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileNameWithoutExtension(path);
                var match = regex.Match(fileName);
                if (!match.Success) continue;

                var sub = fileName.Substring(match.Length);
                if (TryParseLightmapPath(sub, out var lightmapType, out var index))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    result.Add(new Lightmap(lightmapType, asset, path, index));
                }
            }
            return result;
        }

        public static bool TryParseLightmapPath(string name, out string lightmapType, out int index)
        {
            lightmapType = default;
            index = default;
            if (LM0.IsMatch(name))
            {
                index = 0;
            }
            else
            {
                var match = LMA.Match(name);
                if (match.Success)
                {
                    var str = name.Substring(3, match.Length - 3);
                    if (int.TryParse(str, out var i))
                    {
                        index = i;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (Color.IsMatch(name))
            {
                lightmapType = BakeryLightmap.Light;
            }
            else if (Dir.IsMatch(name))
            {
                lightmapType = BakeryLightmap.Directional;
            }
            else if (L0.IsMatch(name))
            {
                lightmapType = BakeryLightmap.L0;
            }
            else if (L1.IsMatch(name))
            {
                lightmapType = BakeryLightmap.L1;
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
