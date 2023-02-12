using System;
using System.IO;
using UnityEditor;

namespace Kanikama.Core.Editor
{
    public sealed class SceneAssetHandler : IDisposable
    {
        public SceneAssetData SceneAssetData { get; }

        public SceneAssetHandler(SceneAssetData sceneAssetData)
        {
            SceneAssetData = sceneAssetData;
        }

        public void Dispose()
        {
            var directory = System.IO.Path.GetDirectoryName(SceneAssetData.Path);
            if (directory != null)
            {
                var lightingDataDirectory = System.IO.Path.Combine(directory, SceneAssetData.Asset.name);
                FileUtil.DeleteFileOrDirectory(lightingDataDirectory);
            }
            AssetDatabase.DeleteAsset(SceneAssetData.Path);
            AssetDatabase.Refresh();
        }
    }
}
