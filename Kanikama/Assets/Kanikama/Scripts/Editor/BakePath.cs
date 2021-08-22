using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanikama.Editor
{
    public class BakePath
    {
        public const string ExportDirFormat = "{0}_Kanikama";
        public const string TmpDirName = "tmp";

        public const string TexArrFormat = "KanikamaTexArray_{0}.asset";
        public const string DirTexArrFormat = "KanikamaDirTexArray_{0}.asset";
        public const string CustomRenderTextureMaterialFormat = "KanikamaCRTComposite_{0}.mat";
        public const string RenderTextureMaterialFormat = "KanikamaRTComposite_{0}.mat";
        public const string CustomRenderTextureFormat = "KanikamaCRT_{0}.asset";
        public const string RenderTextureFormat = "KanikamaRT_{0}.renderTexture";

        public static string AmbientFormat() => $"A_{{0}}.exr";
        public static string LightFormat(int lightIndex) => $"L_{{0}}_{lightIndex}.exr";
        public static string MonitorFormat(int monitorIndex, int lightIndex) => $"M_{{0}}_{monitorIndex}_{lightIndex}.exr";
        public static string RendererFormat(int rendererIndex, int materialIndex) => $"R_{{0}}_{rendererIndex}_{materialIndex}.exr";

        public const string DirectionalPrefix = "Dir-";

        public static readonly Regex UnityLightMapRegex = new Regex("Lightmap-[0-9]+_comp_light.exr");
        public static readonly Regex UnityDirectionalLightMapRegex = new Regex("Lightmap-[0-9]+_comp_dir.png");
        public static readonly Regex TempTextureRegex = new Regex($"^[A-Z]_[0-9]|^{DirectionalPrefix}[A-Z]_[0-9]");
        public static Regex KanikamaRegex(int lightmapIndex) => new Regex($"^[A-Z]_{lightmapIndex}");
        public static Regex KanikamaDirectionalRegex(int lightmapIndex) => new Regex($"^{DirectionalPrefix}[A-Z]_{lightmapIndex}");
        public static readonly Regex KanikamaAssetRegex = new Regex("KanikamaTexArray_[0-9]+.asset|KanikamaComposite_[0-9]+.mat|KanikamaMap_[0-9]+.asset");
        public string UnityLightmapDirPath { get; }
        public string ExportDirPath { get; }
        public string TmpDirPath { get; }


        public BakePath(Scene scene)
        {
            var sceneDirPath = Path.GetDirectoryName(scene.path);
            var exportDirName = string.Format(ExportDirFormat, scene.name);
            AssetUtil.CreateFolderIfNecessary(sceneDirPath, exportDirName);
            ExportDirPath = Path.Combine(sceneDirPath, exportDirName);
            AssetUtil.CreateFolderIfNecessary(ExportDirPath, TmpDirName);
            TmpDirPath = Path.Combine(ExportDirPath, TmpDirName);

            UnityLightmapDirPath = Path.Combine(sceneDirPath, scene.name);
        }

        public List<string> GetUnityLightmapPaths()
        {
            return AssetDatabase.FindAssets("t:Texture", new string[1] { UnityLightmapDirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => Path.GetDirectoryName(x) == UnityLightmapDirPath)
                .Where(x => UnityLightMapRegex.IsMatch(x)).ToList();
        }

        public List<string> GetUnityDirectionalLightmapPaths()
        {
            return AssetDatabase.FindAssets("t:Texture", new string[1] { UnityLightmapDirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => Path.GetDirectoryName(x) == UnityLightmapDirPath)
                .Where(x => UnityDirectionalLightMapRegex.IsMatch(x)).ToList();
        }

        public List<TempTexturePath> GetAllTempTexturePaths()
        {
            return AssetDatabase.FindAssets("t:Texture", new string[1] { TmpDirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => Path.GetDirectoryName(x) == TmpDirPath)
                .Where(x => TempTextureRegex.IsMatch(Path.GetFileNameWithoutExtension(x)))
                .Select(x => new TempTexturePath(x))
                .ToList();
        }

        public List<Texture2D> LoadKanikamaMaps(int lightmapIndex)
        {
            var regex = KanikamaRegex(lightmapIndex);
            return AssetDatabase.FindAssets("t:Texture", new string[1] { TmpDirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => Path.GetDirectoryName(x) == TmpDirPath)
                .Where(x => regex.IsMatch(Path.GetFileName(x)))
                .Select(x => AssetDatabase.LoadAssetAtPath<Texture2D>(x))
                .ToList();
        }

        public List<Texture2D> LoadKanikamaDirectionalMaps(int lightmapIndex)
        {
            var regex = KanikamaDirectionalRegex(lightmapIndex);
            return AssetDatabase.FindAssets("t:Texture", new string[1] { TmpDirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => Path.GetDirectoryName(x) == TmpDirPath)
                .Where(x => regex.IsMatch(Path.GetFileName(x)))
                .Select(x => AssetDatabase.LoadAssetAtPath<Texture2D>(x))
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
            public BakeTargetType Type { get; }
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
                    case "A":
                        Type = BakeTargetType.Ambient;
                        break;
                    case "L":
                        Type = BakeTargetType.Light;
                        ObjectIndex = int.Parse(list[2]);
                        break;
                    case "M":
                        Type = BakeTargetType.Moitor;
                        ObjectIndex = int.Parse(list[2]);
                        SubIndex = int.Parse(list[3]);
                        break;
                    case "R":
                        Type = BakeTargetType.Renderer;
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
            Ambient = 1,
            Light = 2,
            Moitor = 3,
            Renderer = 4,
        }
    }
}
