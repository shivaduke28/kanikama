using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace Kanikama.Editor
{
    public class Baker
    {
        public const string ExportDirFormat = "{0}_Kanikama";
        const string TmpDirName = "tmp";

        // const string KLmapFormat = "KL-{0}_{1}.exr";
        // const string KRmapFormat = "KR-{0}-{1}_{2}.exr";
        // const string KAmapFormat = "KA-{0}.exr";
        // const string KMmapFormat = "KM-{0}-{1}_{2}.exr";

        const string KLTexArrFormat = "KL-array-{0}.asset";
        const string KLCompositeMatFormat = "KL-mat-comp-{0}.mat";
        const string KLCompositeMapFormat = "KL-comp-{0}.asset";

        public const string CompositeShaderName = "Kanikama/Composite";
        public const string DummyShaderName = "Kanikama/Dummy";

        static class ShaderProperties
        {
            public static readonly int TexCount = Shader.PropertyToID("_TexCount");
            public static readonly int TexArray = Shader.PropertyToID("_Tex2DArray");
        }

        static string LightFormat(int lightIndex) => $"KL-{{0}}_{lightIndex}.exr";
        static string RendererFormat(int rendererIndex, int materialIndex) => $"KM-{{0}}_{rendererIndex}_{materialIndex}.exr";
        static string AmbientFormat() => $"KA-{{0}}.exr";
        static string MonitorFormat(int monitorIndex, int lightIndex) => $"KM-{{0}}_{monitorIndex}_{lightIndex}.exr";
        static Regex KanikamaRegex(int lightmapIndex) => new Regex($"^[A-Z]+-{lightmapIndex}");
        static readonly Regex LightMapRegex = new Regex("Lightmap-[0-9]+_comp_light.exr");
        //const string LightmapFormat = "Lightmap-{0}_comp_light.exr";


        readonly BakeSceneController sceneController;
        readonly BakeRequest request;
        string bakedAssetsDirPath;
        string exportDirPath;
        string tmpDirPath;

        public Baker(BakeRequest request)
        {
            this.request = request;
            sceneController = new BakeSceneController(request.SceneDescriptor);
        }

        public async Task BakeAsync(CancellationToken token)
        {
            try
            {
                Debug.Log($"v..v Bake Start v..v");

                SetUpDirectories();
                sceneController.Initialize();
                sceneController.TurnOff();
                await BakeLightsAsync(token);
                await BakeEmissiveRenderersAsync(token);
                await BakeMonitorsAsync(token);
                await BakeAmbientAsync(token);
                CreateKanikamaAssets();
                await BakeWithoutKanikamaAsync(token);

                Debug.Log($"v..v Done v..v");
            }
            catch (TaskCanceledException e)
            {
                throw e;
            }
            finally
            {
                sceneController?.Rollback();
                sceneController?.Dispose();
                AssetDatabase.Refresh();
            }
        }

        void SetUpDirectories()
        {
            var scene = SceneManager.GetActiveScene();
            var sceneDirPath = Path.GetDirectoryName(scene.path);
            var exportDirName = string.Format(ExportDirFormat, scene.name);

            AssetUtil.CreateFolderIfNecessary(sceneDirPath, exportDirName);
            exportDirPath = Path.Combine(sceneDirPath, exportDirName);
            AssetUtil.CreateFolderIfNecessary(exportDirPath, TmpDirName);
            tmpDirPath = Path.Combine(exportDirPath, TmpDirName);

            bakedAssetsDirPath = Path.Combine(sceneDirPath, scene.name.ToLower());
        }

        async Task BakeLightsAsync(CancellationToken token)
        {
            for (var i = 0; i < sceneController.KanikamaLights.Count; i++)
            {
                if (!request.IsLightRequested(i)) continue;
                await BakeLightAsync(i, token);
            }
        }

        async Task BakeLightAsync(int index, CancellationToken token)
        {
            var light = sceneController.KanikamaLights[index];
            Debug.Log($"Baking Light {light.Name}");
            light.OnBake();
            await BakeSceneGIAsync(token);
            light.TurnOff();
            MoveBakedLightmaps(LightFormat(index));
        }

        async Task BakeEmissiveRenderersAsync(CancellationToken token)
        {
            for (var i = 0; i < sceneController.KanikamaEmissiveRenderers.Count; i++)
            {
                if (!request.IsRendererRequested(i)) continue;
                await BakeEmissiveRendererAsync(i, token);
            }
        }

        async Task BakeEmissiveRendererAsync(int rendererIndex, CancellationToken token)
        {
            var renderer = sceneController.KanikamaEmissiveRenderers[rendererIndex];
            Debug.Log($"Baking Renderer {renderer.Name}");

            var matCount = renderer.EmissiveMaterial.Count;
            for (var j = 0; j < matCount; j++)
            {
                var material = renderer.EmissiveMaterial[j];
                Debug.Log($"- Baking Renderer {renderer.Name}'s material {material.Name}");
                material.OnBake();
                Lightmapping.Clear();
                await BakeSceneGIAsync(token);
                material.TurnOff();
                MoveBakedLightmaps(RendererFormat(rendererIndex, j));
            }
        }

        async Task BakeMonitorAsync(int monitorIndex, CancellationToken token)
        {
            var monitor = sceneController.KanikamaMonitors[monitorIndex];
            Debug.Log($"Baking Monitor {monitor.Renderer.name}");
            var lightCount = monitor.Lights.Count;
            for (var i = 0; i < lightCount; i++)
            {
                Debug.Log($"- Baking Monitor {monitor.Renderer.name}'s {i}-th Light");
                var light = monitor.Lights[i];
                light.enabled = true;
                await BakeSceneGIAsync(token);
                light.enabled = false;
                MoveBakedLightmaps(MonitorFormat(monitorIndex, i));
            }
        }

        async Task BakeMonitorsAsync(CancellationToken token)
        {
            for (var i = 0; i < sceneController.KanikamaMonitors.Count; i++)
            {
                if (!request.IsMonitorRequested(i)) continue;
                await BakeMonitorAsync(i, token);
            }
        }

        async Task BakeAmbientAsync(CancellationToken token)
        {
            if (!sceneController.IsKanikamaAmbientEnable) return;
            if (!request.IsBakeAmbient()) return;

            Debug.Log($"Baking Ambient...");
            sceneController.OnAmbientBake();
            await BakeSceneGIAsync(token);
            sceneController.TurnOffAmbient();
            MoveBakedLightmaps(AmbientFormat());
        }

        async Task BakeWithoutKanikamaAsync(CancellationToken token)
        {
            if (!request.IsBakeWithouKanikama()) return;
            sceneController.RollbackNonKanikama();
            Debug.Log($"Baking Scene GI without Kanikama...");
            await BakeSceneGIAsync(token);
        }

        void MoveBakedLightmaps(string format)
        {
            var bakedLightmapPaths = GetBakedLightmapPaths(bakedAssetsDirPath);
            for (var mapIndex = 0; mapIndex < bakedLightmapPaths.Count; mapIndex++)
            {
                var bakedMapPath = bakedLightmapPaths[mapIndex];
                var copiedMapPath = Path.Combine(tmpDirPath, string.Format(format, mapIndex));
                AssetDatabase.CopyAsset(bakedMapPath, copiedMapPath);

                var textureImporter = AssetImporter.GetAtPath(copiedMapPath) as TextureImporter;
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();
            }
        }

        static async Task BakeSceneGIAsync(CancellationToken token)
        {
            Lightmapping.BakeAsync();
            while (Lightmapping.isRunning)
            {
                try
                {
                    await Task.Delay(33, token);
                }
                catch (TaskCanceledException e)
                {
                    Lightmapping.Cancel();
                    throw e;
                }
            }
        }

        void CreateKanikamaAssets()
        {
            if (!request.isGenerateAssets) return;
            var lightMapCount = GetBakedLightmapPaths(bakedAssetsDirPath).Count;
            for (var mapIndex = 0; mapIndex < lightMapCount; mapIndex++)
            {
                var textures = LoadKanikamaMaps(tmpDirPath, mapIndex);
                if (!textures.Any()) continue;
                var texArr = Texture2DArrayGenerator.Generate(textures);
                var texArrPath = Path.Combine(exportDirPath, string.Format(KLTexArrFormat, mapIndex));
                AssetUtil.CreateOrReplaceAsset(ref texArr, texArrPath);
                Debug.Log($"Create {texArrPath}");

                var shader = Shader.Find(CompositeShaderName);
                var mat = new Material(shader);
                var matPath = Path.Combine(exportDirPath, string.Format(KLCompositeMatFormat, mapIndex));
                AssetUtil.CreateOrReplaceAsset(ref mat, matPath);
                Debug.Log($"Create {matPath}");

                mat.SetInt(ShaderProperties.TexCount, textures.Count);
                mat.SetTexture(ShaderProperties.TexArray, texArr);

                var sceneMapPath = Path.Combine(exportDirPath, string.Format(KLCompositeMapFormat, mapIndex));
                var sceneMap = new CustomRenderTexture(texArr.width, texArr.height, RenderTextureFormat.ARGBHalf)
                {
                    material = mat,
                    updateMode = CustomRenderTextureUpdateMode.Realtime,
                    initializationMode = CustomRenderTextureUpdateMode.OnLoad,
                    initializationColor = Color.black
                };
                AssetUtil.CreateOrReplaceAsset(ref sceneMap, sceneMapPath);
                Debug.Log($"Create {sceneMapPath}");

            }
            AssetDatabase.Refresh();
        }

        static List<string> GetBakedLightmapPaths(string dirPath)
        {
            return AssetDatabase.FindAssets("t:Texture", new string[1] { dirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => LightMapRegex.IsMatch(x)).ToList();
        }

        static List<Texture2D> LoadKanikamaMaps(string dirPath, int lightmapIndex)
        {
            var regex = KanikamaRegex(lightmapIndex);
            return AssetDatabase.FindAssets("t:Texture", new string[1] { dirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => regex.IsMatch(Path.GetFileName(x)))
                .Select(x => AssetDatabase.LoadAssetAtPath<Texture2D>(x))
                .ToList();
        }
    }
}