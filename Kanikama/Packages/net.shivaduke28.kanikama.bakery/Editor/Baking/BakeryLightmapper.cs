#if BAKERY_INCLUDED
using System.Threading;
using System.Threading.Tasks;

namespace Kanikama.Baking.Bakery
{
    public class BakeryLightmapper : ILightmapper
    {
        readonly ftRenderLightmap bakery;

        public BakeryLightmapper()
        {
            bakery = ftRenderLightmap.instance ?? new ftRenderLightmap();
            bakery.LoadRenderSettings();
        }

        public async Task BakeAsync(CancellationToken token)
        {
            ftRenderLightmap.outputPathFull = ftRenderLightmap.outputPath;
            bakery.RenderButton(false);
            while (ftRenderLightmap.bakeInProgress)
            {
                try
                {
                    await Task.Delay(33, token);
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
            }

            if (ftRenderLightmap.userCanceled)
            {
                throw new TaskCanceledException();
            }
        }

        public void Clear()
        {
        }

        public bool IsDirectionalMode()
        {
            return ftRenderLightmap.renderDirMode == ftRenderLightmap.RenderDirMode.DominantDirection;
        }
    }
}
#endif