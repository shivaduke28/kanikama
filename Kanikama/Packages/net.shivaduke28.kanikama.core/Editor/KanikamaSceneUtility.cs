using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kanikama.Core.Editor.LightSources;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

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
            var dirPath = Path.GetDirectoryName(scene.path);
            var lightingDirPath = dirPath != null ? Path.Combine(dirPath, scene.name) : string.Empty;
            sceneAssetData = new SceneAssetData(path, sceneAsset, lightingDirPath);
            return true;
        }

        public static SceneAssetData ToAssetData(SceneAsset sceneAsset)
        {
            var path = AssetDatabase.GetAssetPath(sceneAsset);
            var dirPath = Path.GetDirectoryName(path);
            var lightingDirPath = dirPath != null ? Path.Combine(dirPath, sceneAsset.name) : string.Empty;
            return new SceneAssetData(path, sceneAsset, lightingDirPath);
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
            var dirPath = Path.GetDirectoryName(newPath);
            var lightingDirPath = dirPath != null ? Path.Combine(dirPath, newAsset.name) : string.Empty;
            return new TemporarySceneAssetHandle(new SceneAssetData(newPath, newAsset, lightingDirPath));
        }

        public static BakedLightingAssetCollection GetBakedAssetData(SceneAssetData sceneAssetData)
        {
            var dirPath = sceneAssetData.LightingAssetDirectoryPath;
            var result = new BakedLightingAssetCollection
            {
                Lightmaps = new List<BakedLightmap>(),
                DirectionalLightmaps = new List<BakedLightmap>(),
                ShadowMasks = new List<BakedLightmap>(),
            };

            foreach (var guid in AssetDatabase.FindAssets("t:Texture", new[] { dirPath }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!TryGetLightmapType(path, out var lightmapType, out var index))
                {
                    continue;
                }
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                var lightmap = new BakedLightmap(lightmapType, texture, path, index);
                switch (lightmapType)
                {
                    case LightmapType.Color:
                        result.Lightmaps.Add(lightmap);
                        break;
                    case LightmapType.Directional:
                        result.DirectionalLightmaps.Add(lightmap);
                        break;
                    case LightmapType.ShadowMask:
                        result.ShadowMasks.Add(lightmap);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return result;
        }

        public static BakedLightmap CopyBakedLightmap(BakedLightmap bakedLightmap, string dstPath)
        {
            AssetDatabase.CopyAsset(bakedLightmap.Path, dstPath);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(dstPath);
            return new BakedLightmap(bakedLightmap.Type, texture, dstPath, bakedLightmap.Index);
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

        public static bool TryGetLightmapType(string path, out LightmapType lightmapType, out int lightmapIndex)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);

            if (!LightmapRegex.IsMatch(fileName))
            {
                lightmapType = default;
                lightmapIndex = -1;
                return false;
            }

            // "Lightmap_".Length = 9
            var str = fileName.Substring(9);

            // length should be 3
            var list = str.Split("_".ToCharArray());

            lightmapIndex = int.Parse(list[0]);
            switch (list[2])
            {
                case "light":
                    lightmapType = LightmapType.Color;
                    break;
                case "dir":
                    lightmapType = LightmapType.Directional;
                    break;
                case "shadowmask":
                    lightmapType = LightmapType.ShadowMask;
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
