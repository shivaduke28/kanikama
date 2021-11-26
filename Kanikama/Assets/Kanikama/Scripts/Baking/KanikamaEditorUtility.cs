using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanikama.Baking
{
    public static class KanikamaEditorUtility
    {
        public static string GetName(object obj)
        {
            if (obj == null) return "null";
            try
            {
                return obj is UnityEngine.Object ob ? ob.name : obj.GetType().Name;
            }
            catch (MissingReferenceException)
            {
                return "missing";
            }
        }

        public static SceneAsset GetActiveSceneAsset()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            return string.IsNullOrEmpty(scene.path) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
        }

        public static bool IsValidPath(string path)
        {
            return AssetDatabase.LoadMainAssetAtPath(path) != null;
        }

        public static bool TryLoadAsset<T>(string path, out T asset) where T : UnityEngine.Object
        {
            asset = (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
            return asset is null;
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

        public static void CreateFolderIfNecessary(string dirPath, string folderName)
        {
            if (!AssetDatabase.IsValidFolder(Path.Combine(dirPath, folderName)))
            {
                AssetDatabase.CreateFolder(dirPath, folderName);
            }
        }

        // https://answers.unity.com/questions/1415405/how-to-open-assets-directory-in-project-window.html
        public static void OpenDirectory(string assetPath)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            var pt = Type.GetType("UnityEditor.ProjectBrowser,UnityEditor");
            var ins = pt.GetField("s_LastInteractedProjectBrowser", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            var showDirMeth = pt.GetMethod("ShowFolderContents", BindingFlags.NonPublic | BindingFlags.Instance);
            showDirMeth.Invoke(ins, new object[] { asset.GetInstanceID(), true });
        }
    }
}