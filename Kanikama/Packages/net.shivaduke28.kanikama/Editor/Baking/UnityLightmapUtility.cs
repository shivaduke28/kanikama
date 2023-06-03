using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.Baking.Util
{
    public static class UnityLightmapUtility
    {
        public static readonly Regex FileNameRegex = new Regex("Lightmap-[0-9]+_comp_[light|dir|shadowmask]");

        public static bool TryGetLightmapType(string assetPath, out UnityLightmapType unityLightmapType, out int lightmapIndex)
        {
            var fileName = Path.GetFileNameWithoutExtension(assetPath);

            if (!FileNameRegex.IsMatch(fileName))
            {
                unityLightmapType = default;
                lightmapIndex = -1;
                return false;
            }

            // "Lightmap-".Length = 9
            var str = fileName.Substring(9);

            // length should be 3
            var list = str.Split("_".ToCharArray());

            lightmapIndex = int.Parse(list[0]);
            switch (list[2])
            {
                case "light":
                    unityLightmapType = UnityLightmapType.Light;
                    break;
                case "dir":
                    unityLightmapType = UnityLightmapType.Directional;
                    break;
                case "shadowmask":
                    unityLightmapType = UnityLightmapType.ShadowMask;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(list[2]);
            }
            return true;
        }

        public static List<UnityLightmap> GetLightmaps(SceneAssetData sceneAssetData)
        {
            var dirPath = sceneAssetData.LightingAssetDirectoryPath;
            var result = new List<UnityLightmap>();

            foreach (var guid in AssetDatabase.FindAssets("t:Texture", new[] { dirPath }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!TryGetLightmapType(path, out var lightmapType, out var index))
                {
                    continue;
                }
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                var lightmap = new UnityLightmap(lightmapType, texture, path, index);
                result.Add(lightmap);
            }
            return result;
        }

        public static UnityLightmap CopyBakedLightmap(UnityLightmap unityLightmap, string dstPath)
        {
            AssetDatabase.CopyAsset(unityLightmap.Path, dstPath);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(dstPath);
            return new UnityLightmap(unityLightmap.Type, texture, dstPath, unityLightmap.Index);
        }

        public static string ToFileName(this UnityLightmapType unityLightmapType)
        {
            switch (unityLightmapType)
            {
                case UnityLightmapType.Light:
                    return "light";
                case UnityLightmapType.Directional:
                    return "dir";
                case UnityLightmapType.ShadowMask:
                    return "shadowmask";
                default:
                    throw new ArgumentOutOfRangeException(nameof(unityLightmapType), unityLightmapType, null);
            }
        }
    }
}
