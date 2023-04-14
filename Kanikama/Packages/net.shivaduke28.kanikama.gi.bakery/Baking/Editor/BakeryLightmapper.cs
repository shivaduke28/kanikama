using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

namespace Kanikama.GI.Bakery.Editor
{
    public sealed class BakeryLightmapper
    {
        readonly ftRenderLightmap bakery;

        public BakeryLightmapper()
        {
            bakery = ftRenderLightmap.instance != null ? ftRenderLightmap.instance : EditorWindow.GetWindow<ftRenderLightmap>();
            bakery.LoadRenderSettings();
        }

        public string OutputAssetDirPath => Path.Combine("Assets", ftRenderLightmap.outputPath);

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
