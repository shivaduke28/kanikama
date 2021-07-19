using UnityEditor;
using UnityEngine;
using System.IO;

namespace Kanikama.Editor
{
    public static class AssetUtil
    {

        public static bool IsValidPath(string path)
        {
            return AssetDatabase.LoadMainAssetAtPath(path) != null;
        }

        public static bool TryLoadAsset<T>(string path, out T asset) where T : Object
        {
            asset = (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
            return asset is null;
        }

        public static void CreateOrReplaceAsset<T>(ref T asset, string path) where T : Object
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

        public static void CreateFolderIfNecessary(string dirPath, string folderName)
        {
            if (!AssetDatabase.IsValidFolder(Path.Combine(dirPath, folderName)))
            {
                AssetDatabase.CreateFolder(dirPath, folderName);
            }
        }
    }
}