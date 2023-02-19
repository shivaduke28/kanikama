using System;
using System.IO;
using UnityEditor;

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
            var directory = Path.GetDirectoryName(SceneAssetData.Path);
            if (directory != null)
            {
                var lightingDataDirectory = Path.Combine(directory, SceneAssetData.Asset.name);
                FileUtil.DeleteFileOrDirectory(lightingDataDirectory);
            }
            AssetDatabase.DeleteAsset(SceneAssetData.Path);
            AssetDatabase.Refresh();
        }
    }
}
