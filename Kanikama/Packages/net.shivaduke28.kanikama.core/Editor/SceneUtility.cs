using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Kanikama.Core.Editor
{
    public sealed class SceneLightingData
    {
        public AmbientLightHandler AmbientLight { get; } = new AmbientLightHandler();
        public List<LightHandler> Light { get; } = new List<LightHandler>();
        public List<RendererHandler> Renderers { get; } = new List<RendererHandler>();
    }

    public struct TextureAssetData
    {
        public string Path;
        public Texture2D Asset;
    }


    public static class SceneUtility
    {
        public static SceneAssetHandler CopySceneAsset(SceneAssetData sceneAssetData)
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
            return new SceneAssetHandler(new SceneAssetData(newPath, newAsset, lightingDirPath));
        }


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


        public static void TurnOffAllLightSources()
        {
            var lights = Object.FindObjectsOfType<Light>();
            foreach (var light in lights)
            {
                if (IsContributeGI(light))
                {
                    light.enabled = false;
                }
            }
            var renderers = Object.FindObjectsOfType<Renderer>();
            foreach (var renderer in renderers)
            {
                if (IsContributeGI(renderer))
                {
                }
            }
        }

        public static bool IsContributeGI(Light light)
        {
            return light.isActiveAndEnabled && (light.lightmapBakeType & LightmapBakeType.Realtime) == 0;
        }

        public static bool IsContributeGI(Renderer renderer)
        {
            if (!renderer.enabled) return false;
            var flags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
            return flags.HasFlag(StaticEditorFlags.ContributeGI) && renderer.sharedMaterials.Any(IsContributeGI);
        }

        public static bool IsContributeGI(Material material)
        {
            return material.globalIlluminationFlags.HasFlag(MaterialGlobalIlluminationFlags.BakedEmissive);
        }


        public static BakedLightmap[] GetBakedLightmaps(SceneAssetData sceneAssetData)
        {
            var dirPath = sceneAssetData.LightingAssetDirectoryPath;
            var result = new List<BakedLightmap>();

            foreach (var guid in AssetDatabase.FindAssets("t:Texture", new string[] { dirPath }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!TryGetLightmapType(path, out var lightmapType, out var index))
                {
                    continue;
                }
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                result.Add(new BakedLightmap(lightmapType, texture, path, index));
            }

            return result.ToArray();
        }

        // todo: name control...
        public static BakedLightmap CopyBakedLightmap(BakedLightmap bakedLightmap, string dstDirPath)
        {
            var newPath = Path.Combine(dstDirPath, Path.GetFileName(bakedLightmap.Path));
            AssetDatabase.CopyAsset(bakedLightmap.Path, newPath);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(newPath);
            return new BakedLightmap(bakedLightmap.Type, texture, newPath, bakedLightmap.Index);
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


        static readonly Regex LightmapRegex = new Regex("Lightmap-[0-9]+_comp_[light|dir]");

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
                default:
                    throw new ArgumentOutOfRangeException(list[2]);
            }
            return true;
        }
    }
}
