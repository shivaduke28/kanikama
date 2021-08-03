using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanikama.Editor
{
    public class BakePath
    {
        public const string ExportDirFormat = "{0}_Kanikama";
        public const string TmpDirName = "tmp";

        public const string TexArrFormat = "KanikamaTexArray-{0}.asset";
        public const string CompositeMaterialFormat = "KanikamaComposite-{0}.mat";
        public const string KanikamaMapFormat = "KanikamaMap-{0}.asset";

        public static string AmbientFormat() => $"A_{{0}}.exr";
        public static string LightFormat(int lightIndex) => $"L_{{0}}_{lightIndex}.exr";
        public static string MonitorFormat(int monitorIndex, int lightIndex) => $"M_{{0}}_{monitorIndex}_{lightIndex}.exr";
        public static string RendererFormat(int rendererIndex, int materialIndex) => $"R_{{0}}_{rendererIndex}_{materialIndex}.exr";

        public static readonly Regex UnityLightMapRegex = new Regex("Lightmap-[0-9]+_comp_light.exr");
        public static readonly Regex TempTextureRegex = new Regex("^[A-Z]_[0-9]");
        public static Regex KanikamaRegex(int lightmapIndex) => new Regex($"^[A-Z]+_{lightmapIndex}");
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

            UnityLightmapDirPath = Path.Combine(sceneDirPath, scene.name.ToLower());
        }

        public List<string> GetUnityLightmapPaths()
        {
            return AssetDatabase.FindAssets("t:Texture", new string[1] { UnityLightmapDirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => UnityLightMapRegex.IsMatch(x)).ToList();
        }

        public List<TempTexturePath> GetAllTempTexturePaths()
        {
            return AssetDatabase.FindAssets("t:Texture", new string[1] { TmpDirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => TempTextureRegex.IsMatch(Path.GetFileNameWithoutExtension(x)))
                .Select(x => new TempTexturePath(x))
                .ToList();
        }

        public List<Texture2D> LoadKanikamaMaps(int lightmapIndex)
        {
            var regex = KanikamaRegex(lightmapIndex);
            return AssetDatabase.FindAssets("t:Texture", new string[1] { TmpDirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => regex.IsMatch(Path.GetFileName(x)))
                .Select(x => AssetDatabase.LoadAssetAtPath<Texture2D>(x))
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

            public TempTexturePath(string path)
            {
                FileName = System.IO.Path.GetFileNameWithoutExtension(path);
                this.Path = path;
                var list = FileName.Split("_".ToCharArray());
                var typeText = list[0];
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

        public enum BakeTargetType
        {
            Ambient = 1,
            Light = 2,
            Moitor = 3,
            Renderer = 4,
        }
    }
}
