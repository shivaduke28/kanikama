using System.IO;
using System.Threading;
using System.Threading.Tasks;
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

        public string OutputAssetDirPath => Path.Combine("Assets", ftRenderLightmap.outputPath);
        public bool UseMipmap => projectSetting.mipmapLightmaps;

        public async Task BakeAsync(CancellationToken cancellationToken)
        {
            ftRenderLightmap.outputPathFull = ftRenderLightmap.outputPath;
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
