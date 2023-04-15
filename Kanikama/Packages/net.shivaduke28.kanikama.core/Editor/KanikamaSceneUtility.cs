﻿using System;
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

        public static TemporarySceneAssetHandle CopySceneAsset(SceneAssetData sceneAssetData)
        {
            var path = sceneAssetData.Path;
            var dir = Path.GetDirectoryName(path);
            if (dir == null)
            {
                throw new Exception("Directory is not found");
            }
            var file = Path.GetFileNameWithoutExtension(path);
            var newPath = Path.Combine(dir, $"{file}_copy.unity");
            AssetDatabase.CopyAsset(path, newPath);
            var newAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(newPath);
            return new TemporarySceneAssetHandle(new SceneAssetData(newPath, newAsset, LightingAssetDirPath(newAsset)));
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

        public static void CreateFolderIfNecessary(string dirPath, string folderName)
        {
            if (!AssetDatabase.IsValidFolder(Path.Combine(dirPath, folderName)))
            {
                AssetDatabase.CreateFolder(dirPath, folderName);
            }
        }

        // start with "Assets/"
        public static void CreateFolderIfNecessary(string dirPath)
        {
            if (!AssetDatabase.IsValidFolder(dirPath))
            {
                var parent = Path.GetDirectoryName(dirPath);
                var folderName = dirPath.Substring(parent.Length + 1);
                AssetDatabase.CreateFolder(parent, folderName);
            }
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
