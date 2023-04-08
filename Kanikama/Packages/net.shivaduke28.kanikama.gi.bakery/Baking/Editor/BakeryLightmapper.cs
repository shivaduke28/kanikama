using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core.Editor;

namespace Kanikama.GI.Bakery.Editor
{
    public sealed class BakeryLightmapper : ILightmapper
    {
        readonly ftRenderLightmap bakery;

        public BakeryLightmapper()
        {
            bakery = ftRenderLightmap.instance;
            bakery.LoadRenderSettings();
        }

        public void ClearCache()
        {
        }

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
