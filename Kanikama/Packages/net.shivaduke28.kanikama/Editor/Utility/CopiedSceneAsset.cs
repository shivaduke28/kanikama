using System;
using System.IO;
using UnityEditor;

namespace Kanikama.Utility.Editor
{
    public sealed class CopiedSceneAsset : IDisposable
    {
        public SceneAssetData SceneAssetData { get; }
        readonly bool disposeLightingAssetDir;

        public CopiedSceneAsset(SceneAssetData sceneAssetData, bool disposeLightingAssetDir = false)
        {
            SceneAssetData = sceneAssetData;
            this.disposeLightingAssetDir = disposeLightingAssetDir;
        }

        public void Dispose()
        {
            if (disposeLightingAssetDir)
            {
                FileUtil.DeleteFileOrDirectory(SceneAssetData.LightingAssetDirectoryPath);
            }
            AssetDatabase.DeleteAsset(SceneAssetData.Path);
            AssetDatabase.Refresh();
        }

        public static CopiedSceneAsset Create(SceneAssetData sceneAssetData, bool willDeleteLightingAssetDir = false, string suffix = "_copy")
        {
            var path = sceneAssetData.Path;
            var dir = Path.GetDirectoryName(path);
            if (dir == null)
            {
                throw new Exception("Directory is not found");
            }
            var name = Path.GetFileNameWithoutExtension(path);
            var newPath = Path.Combine(dir, $"{name}{suffix}.unity");
            AssetDatabase.CopyAsset(path, newPath);
            var newAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(newPath);
            return new CopiedSceneAsset(new SceneAssetData(newAsset), willDeleteLightingAssetDir);
        }
    }
}
