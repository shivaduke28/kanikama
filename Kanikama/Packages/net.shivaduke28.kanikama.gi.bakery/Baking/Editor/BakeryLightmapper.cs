using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core.Editor;
using UnityEditor;

namespace Kanikama.GI.Bakery.Editor
{
    [InitializeOnLoad]
    public sealed class BakeryLightmapper : ILightmapper
    {
        static ILightmapper Create() => new BakeryLightmapper();

        static BakeryLightmapper()
        {
            LightmapperFactory.Register("Bakery", Create, 1);
        }

        readonly ftRenderLightmap bakery;

        public BakeryLightmapper()
        {
            bakery = ftRenderLightmap.instance != null ? ftRenderLightmap.instance : EditorWindow.GetWindow<ftRenderLightmap>();
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
