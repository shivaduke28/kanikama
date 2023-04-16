﻿using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core.Editor;
using UnityEditor;

namespace Kanikama.GI.Bakery.Editor
{
    public sealed class BakeryLightmapper
    {
        readonly ftRenderLightmap bakery;
        readonly BakeryProjectSettings projectSetting;

        public BakeryLightmapper()
        {
            bakery = ftRenderLightmap.instance != null ? ftRenderLightmap.instance : EditorWindow.GetWindow<ftRenderLightmap>();
            bakery.LoadRenderSettings();
            projectSetting = ftLightmaps.GetProjectSettings();
        }

        public void SetOutputAssetDirPath(string outputAssetDirPath)
        {
            ftRenderLightmap.useScenePath = false;
            ftRenderLightmap.outputPath = KanikamaSceneUtility.RemoveAssetsFromPath(outputAssetDirPath);
        }

        public bool UseMipmap => projectSetting.mipmapLightmaps;

        public async Task BakeAsync(CancellationToken cancellationToken)
        {
            bakery.RenderButton(false);
            while (ftRenderLightmap.bakeInProgress)
            {
                await Task.Delay(33, cancellationToken);
            }

            if (ftRenderLightmap.userCanceled)
            {
                throw new TaskCanceledException();
            }
        }
    }
}
