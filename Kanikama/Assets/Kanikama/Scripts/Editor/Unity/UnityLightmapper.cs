using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Kanikama.Editor
{

    public class UnityLightmapper : ILightmapper
    {
        static readonly Regex UnityLightMapRegex = new Regex("Lightmap-[0-9]+_comp_light.exr");
        static readonly Regex UnityDirectionalLightMapRegex = new Regex("Lightmap-[0-9]+_comp_dir.png");
        readonly string lightmapDirPath;

        public UnityLightmapper(Scene scene)
        {
            var sceneDirPath = Path.GetDirectoryName(scene.path);
            lightmapDirPath = Path.Combine(sceneDirPath, scene.name);
        }

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
            Lightmapping.Cancel();
        }

        public bool IsLightMap(string assetPath)
        {
            return UnityLightMapRegex.IsMatch(assetPath);
        }
        public bool IsDirectionalMap(string assetPath)
        {
            return UnityDirectionalLightMapRegex.IsMatch(assetPath);
        }

        public string LightMapDirPath()
        {
            return lightmapDirPath;
        }
    }
}