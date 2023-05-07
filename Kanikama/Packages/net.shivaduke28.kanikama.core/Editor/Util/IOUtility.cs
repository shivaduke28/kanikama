using System;
using System.IO;
using UnityEditor;

namespace Kanikama.Core.Editor.Util
{
    public static class IOUtility
    {
        public static void CreateFolderIfNecessary(string assetDirPath, string folderName)
        {
            if (!AssetDatabase.IsValidFolder(Path.Combine(assetDirPath, folderName)))
            {
                AssetDatabase.CreateFolder(assetDirPath, folderName);
            }
        }

        public static void CreateFolderIfNecessary(string assetDirPath)
        {
            if (!AssetDatabase.IsValidFolder(assetDirPath))
            {
                var parent = Path.GetDirectoryName(assetDirPath);
                var folderName = assetDirPath.Substring(parent.Length + 1);
                AssetDatabase.CreateFolder(parent, folderName);
            }
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
    }
}
