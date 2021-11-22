using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Kanikama.Editor.Bakery
{
    public class BakeryLightmapper : ILightmapper
    {
        readonly ftRenderLightmap bakery;
        readonly Regex LightMapRegex;
        readonly Regex DirectionalMapRegex;
        readonly string lightMapDirPath;

        public BakeryLightmapper(Scene scene)
        {
            bakery = ftRenderLightmap.instance ?? new ftRenderLightmap();
            bakery.LoadRenderSettings();
            ftRenderLightmap.outputPathFull = ftRenderLightmap.outputPath;
            LightMapRegex = new Regex($"{scene.name}_LM[A-Z]*[0-9]+_final.hdr");
            DirectionalMapRegex = new Regex($"{scene.name}_LM[A-Z]*[0-9]+_dir.tga");
            lightMapDirPath = Path.Combine("Assets", ftRenderLightmap.outputPathFull);
        }

        public async Task BakeAsync(CancellationToken token)
        {
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

        public bool IsLightMap(string assetPath) => LightMapRegex.IsMatch(assetPath);
        public bool IsDirectionalMap(string assetPath) => DirectionalMapRegex.IsMatch(assetPath);
        public string LightMapDirPath() => lightMapDirPath;
    }
}
