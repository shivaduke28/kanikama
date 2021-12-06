using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Baking
{
    public class UnityLightmapper : ILightmapper
    {

        public UnityLightmapper() { }

        public async Task BakeAsync(CancellationToken token)
        {
            if (!Lightmapping.BakeAsync())
            {
                throw new TaskCanceledException("The lightmap bake job did not start successfully.");
            }

            while (Lightmapping.isRunning)
            {
                try
                {
                    await Task.Delay(33, token);
                }
                catch (TaskCanceledException)
                {
                    Lightmapping.Cancel();
                    throw;
                }
            }
        }

        public void Clear()
        {
            Lightmapping.Clear();
        }

        public bool IsDirectionalMode()
        {
            return LightmapEditorSettings.lightmapsMode == LightmapsMode.CombinedDirectional;
        }
    }
}