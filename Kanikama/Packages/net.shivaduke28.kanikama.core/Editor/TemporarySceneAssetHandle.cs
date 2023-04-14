using System;
using UnityEditor;

namespace Kanikama.Core.Editor
{
    // TODO: 名前変えたい
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
