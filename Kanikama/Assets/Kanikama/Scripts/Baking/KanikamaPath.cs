using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanikama.Baking
{
    public class KanikamaPath
    {
        public const string ExportDirFormat = "{0}_Kanikama";
        public const string TmpDirName = "tmp";

        // Kanikama
        public const string TexArrFormat = "KanikamaTexArray_{0}.asset";
        public const string DirTexArrFormat = "KanikamaDirTexArray_{0}.asset";
        public const string CustomRenderTextureMaterialFormat = "KanikamaCRTComposite_{0}.mat";
        public const string RenderTextureMaterialFormat = "KanikamaRTComposite_{0}.mat";
        public const string CustomRenderTextureFormat = "KanikamaCRT_{0}.asset";
        public const string RenderTextureFormat = "KanikamaRT_{0}.renderTexture";
        public static readonly Regex KanikamaAssetRegex = new Regex("KanikamaTexArray_[0-9]+.asset|KanikamaComposite_[0-9]+.mat|KanikamaMap_[0-9]+.asset");

        // Temp
        public static string LightSourceFormat(int sourceIndex) => $"L_{{0}}_{sourceIndex}.exr";
        public static string LightSourceGroupFormat(int groupIndex, int sourceIndex) => $"LG_{{0}}_{groupIndex}_{sourceIndex}.exr";
        public const string DirectionalPrefix = "Dir-";
        public static Regex TempLightmapRegex(int lightmapIndex) => new Regex($"^[A-Z]+_{lightmapIndex}");
        public static Regex TempDirectionalMapRegex(int lightmapIndex) => new Regex($"^{DirectionalPrefix}[A-Z]+_{lightmapIndex}");


        public string ExportDirPath { get; }
        public string TmpDirPath { get; }

        public static string KanikamaAssetDirPath(SceneAsset scene)
        {
            var scenePath = AssetDatabase.GetAssetPath(scene);
            var sceneDirPath = Path.GetDirectoryName(scenePath);
            var exportDirName = string.Format(ExportDirFormat, scene.name);
            KanikamaEditorUtility.CreateFolderIfNecessary(sceneDirPath, exportDirName);
            return Path.Combine(sceneDirPath, exportDirName);
        }

        public KanikamaPath(Scene scene)
        {
            var sceneDirPath = Path.GetDirectoryName(scene.path);
            var exportDirName = string.Format(ExportDirFormat, scene.name);
            KanikamaEditorUtility.CreateFolderIfNecessary(sceneDirPath, exportDirName);
            ExportDirPath = Path.Combine(sceneDirPath, exportDirName);
            KanikamaEditorUtility.CreateFolderIfNecessary(ExportDirPath, TmpDirName);
            TmpDirPath = Path.Combine(ExportDirPath, TmpDirName);
        }

        public List<string> GetBakedLightmapPaths()
        {
            return LightmapSettings.lightmaps.Select(x => AssetDatabase.GetAssetPath(x.lightmapColor)).ToList();
        }

        public List<string> GetBakedDirectionalMapPaths()
        {
            return LightmapSettings.lightmaps.Select(x => AssetDatabase.GetAssetPath(x.lightmapDir)).ToList();
        }

        public List<TempTexturePath> GetTempLightmapPaths(int lightmapIndex)
        {
            var regex = TempLightmapRegex(lightmapIndex);
            return AssetDatabase.FindAssets("t:Texture", new string[1] { TmpDirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => Path.GetDirectoryName(x) == TmpDirPath)
                .Where(x => regex.IsMatch(Path.GetFileName(x)))
                .Select(x => new TempTexturePath(x))
                .ToList();
        }

        public List<TempTexturePath> GetTempDirctionalMapPaths(int lightmapIndex)
        {
            var regex = TempDirectionalMapRegex(lightmapIndex);
            return AssetDatabase.FindAssets("t:Texture", new string[1] { TmpDirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => Path.GetDirectoryName(x) == TmpDirPath)
                .Where(x => regex.IsMatch(Path.GetFileName(x)))
                .Select(x => new TempTexturePath(x))
                .ToList();
        }

        public List<KanikamaAssetPath> GetAllKanikamaAssetPaths()
        {
            return AssetDatabase.FindAssets("t:Object", new string[1] { ExportDirPath })
                  .Select(x => AssetDatabase.GUIDToAssetPath(x))
                  .Where(x => Path.GetDirectoryName(x) == ExportDirPath)
                  .Where(x => KanikamaAssetRegex.IsMatch(Path.GetFileName(x)))
                  .Select(x => new KanikamaAssetPath(x))
                  .ToList();
        }

        public class TempTexturePath
        {
            public BakeTargetType Type;
            public int LightmapIndex { get; }
            public int ObjectIndex { get; }
            public int SubIndex { get; }
            public string Path { get; }
            public string FileName { get; }
            public bool IsDirectional { get; }

            public TempTexturePath(string path)
            {
                FileName = System.IO.Path.GetFileNameWithoutExtension(path);
                Path = path;
                var list = FileName.Split("_".ToCharArray());
                var typeText = list[0];

                if (typeText.StartsWith(DirectionalPrefix))
                {
                    IsDirectional = true;
                    typeText = typeText.Split("-".ToCharArray())[1];
                }

                LightmapIndex = int.Parse(list[1]);
                switch (typeText)
                {
                    case "L":
                        Type = BakeTargetType.LightSource;
                        break;
                    case "LG":
                        Type = BakeTargetType.LightSourceGroup;
                        ObjectIndex = int.Parse(list[2]);
                        SubIndex = int.Parse(list[3]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public class KanikamaAssetPath
        {
            public string Path { get; }
            public int LightmapIndex { get; }
            public KanikamaAssetPath(string path)
            {
                Path = path;
                var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                var list = fileName.Split("_".ToCharArray());
                LightmapIndex = int.Parse(list[1]);
            }
        }

        public enum BakeTargetType
        {
            LightSource = 1,
            LightSourceGroup = 2,
        }
    }
}
