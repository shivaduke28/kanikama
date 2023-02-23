using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Core.Editor
{
    public sealed class TemporarySceneAssetHandle : IDisposable
    {
        public SceneAssetData SceneAssetData { get; }

        public TemporarySceneAssetHandle(SceneAssetData sceneAssetData)
        {
            SceneAssetData = sceneAssetData;
        }

        public void Dispose()
        {
            FileUtil.DeleteFileOrDirectory(SceneAssetData.LightingAssetDirectoryPath);
            AssetDatabase.DeleteAsset(SceneAssetData.Path);
            AssetDatabase.Refresh();
        }
    }
}