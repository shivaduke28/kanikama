using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanikama.Core.Editor
{
    public static class KanikamaSceneUtility
    {
        public static bool TryGetActiveSceneAsset(out SceneAssetData sceneAssetData)
        {
            var scene = SceneManager.GetActiveScene();
            var path = scene.path;
            if (string.IsNullOrEmpty(path))
            {
                sceneAssetData = new SceneAssetData(path, null, null);
                return false;
            }
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
            sceneAssetData = new SceneAssetData(path, sceneAsset, LightingAssetDirPath(sceneAsset));
            return true;
        }

        public static string LightingAssetDirPath(SceneAsset sceneAsset)
        {
            var path = AssetDatabase.GetAssetPath(sceneAsset);
            var dirPath = Path.GetDirectoryName(path);
            return dirPath != null ? Path.Combine(dirPath, sceneAsset.name) : string.Empty;
        }

        public static SceneAssetData ToAssetData(SceneAsset sceneAsset)
        {
            var path = AssetDatabase.GetAssetPath(sceneAsset);
            return new SceneAssetData(path, sceneAsset, LightingAssetDirPath(sceneAsset));
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

        // start with "Assets/"
        public static void CreateFolderIfNecessary(string assetDirPath, string folderName)
        {
            if (!AssetDatabase.IsValidFolder(Path.Combine(assetDirPath, folderName)))
            {
                AssetDatabase.CreateFolder(assetDirPath, folderName);
            }
        }

        // start with "Assets/"
        public static void CreateFolderIfNecessary(string assetDirPath)
        {
            if (!AssetDatabase.IsValidFolder(assetDirPath))
            {
                var parent = Path.GetDirectoryName(assetDirPath);
                var folderName = assetDirPath.Substring(parent.Length + 1);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        public static string RemoveAssetsFromPath(string assetPath)
        {
            var dirs = assetPath.Split(Path.DirectorySeparatorChar);
            if (dirs[0] == "Assets")
            {
                var sub = new string[dirs.Length - 1];
                Array.Copy(dirs, 1, sub, 0, dirs.Length - 1);
                return string.Join(Path.DirectorySeparatorChar.ToString(), sub);
            }
            return assetPath;
        }

        public static string AddAssetsToPath(string path)
        {
            var dirs = path.Split(Path.DirectorySeparatorChar);
            if (dirs[0] != "Assets")
            {
                var newDirs = new string[dirs.Length + 1];
                newDirs[0] = "Assets";
                Array.Copy(dirs, 0, newDirs, 1, dirs.Length);
                return string.Join(Path.DirectorySeparatorChar.ToString(), newDirs);
            }
            return path;
        }

        public static void CreateOrReplaceAsset<T>(ref T asset, string path) where T : UnityEngine.Object
        {
            var existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (existingAsset == null)
            {
                AssetDatabase.CreateAsset(asset, path);
            }
            else
            {
                EditorUtility.CopySerialized(asset, existingAsset);
                asset = existingAsset;
            }
        }


        static readonly Regex LightmapRegex = new Regex("Lightmap-[0-9]+_comp_[light|dir|shadowmask]");

        public static bool TryGetLightmapType(string path, out UnityLightmapType unityLightmapType, out int lightmapIndex)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);

            if (!LightmapRegex.IsMatch(fileName))
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
                    unityLightmapType = UnityLightmapType.Color;
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

        public static T FindObjectOfType<T>()
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
            {
                var child = root.GetComponentInChildren<T>();
                if (child != null)
                {
                    return child;
                }
            }
            return default;
        }
    }
}
