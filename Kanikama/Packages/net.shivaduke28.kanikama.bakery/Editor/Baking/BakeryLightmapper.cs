using System;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Editor;
using Kanikama.Editor.Baking;
using UnityEditor;

namespace Kanikama.Bakery.Editor.Baking
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
            ftRenderLightmap.outputPath = IOUtility.RemoveAssetsFromPath(outputAssetDirPath);
        }

        public bool UseMipmap => projectSetting.mipmapLightmaps;

        public async Task BakeAsync(CancellationToken cancellationToken)
        {
            bakery.RenderButton(false);
            while (ftRenderLightmap.bakeInProgress)
            {
                await Task.Delay(33, cancellationToken);
                if (ftRenderLightmap.userCanceled)
                {
                    throw new OperationCanceledException();
                }
            }
        }

        public DirMode DirectionalMode
        {
            get => (DirMode) ftRenderLightmap.renderDirMode;
            set => ftRenderLightmap.renderDirMode = (ftRenderLightmap.RenderDirMode) value;
        }

        public RenderMode LightmapRenderMode
        {
            get => (RenderMode) ftRenderLightmap.instance.userRenderMode;
            set
            {
                ftRenderLightmap.instance.userRenderMode = (ftRenderLightmap.RenderMode) value;
                ftRenderLightmap.FindRenderSettingsStorage().renderSettingsUserRenderMode = (int) value;
            }
        }

        public enum DirMode
        {
            None = 0,
            BakedNormalMaps = 1,
            DominantDirection = 2,
            RNM = 3,
            SH = 4,
            MonoSH = 6
        };

        public int Bounce
        {
            get => ftRenderLightmap.bounces;
            set => ftRenderLightmap.bounces = value;
        }

        public enum RenderMode
        {
            FullLighting = 0,
            Indirect = 1,
            Shadowmask = 2,
            Subtractive = 3,
            AmbientOcclusionOnly = 4
        };
    }
}
