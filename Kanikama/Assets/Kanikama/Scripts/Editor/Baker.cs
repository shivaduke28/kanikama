using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanikama.Editor
{
    public class Baker
    {
        public static class ShaderName
        {
            public const string Composite = "Kanikama/Composite";
            public const string Dummy = "Kanikama/Dummy";
        }

        public static class ShaderProperties
        {
            public static readonly int TexCount = Shader.PropertyToID("_LightmapCount");
            public static readonly int TexArray = Shader.PropertyToID("_LightmapArray");
        }

        readonly BakeSceneController sceneController;
        readonly BakeRequest request;
        readonly BakePath bakePath;

        public Baker(BakeRequest request)
        {
            this.request = request;
            sceneController = new BakeSceneController(request.SceneDescriptor);
            bakePath = new BakePath(SceneManager.GetActiveScene());
        }

        public async Task BakeAsync(CancellationToken token)
        {
            try
            {
                Debug.Log($"v..v Bake Start v..v");

                sceneController.Initialize();
                sceneController.TurnOff();
                await BakeLightsAsync(token);
                await BakeEmissiveRenderersAsync(token);
                await BakeMonitorsAsync(token);
                await BakeAmbientAsync(token);
                await BakeWithoutKanikamaAsync(token);
                CreateKanikamaAssets();

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
            MoveBakedLightmaps(BakePath.LightFormat(index));
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

            var matCount = renderer.EmissiveMaterials.Count;
            for (var i = 0; i < matCount; i++)
            {
                var material = renderer.EmissiveMaterials[i];
                Debug.Log($"- Baking Renderer {renderer.Name}'s material {material.Name}");
                material.OnBake();
                Lightmapping.Clear();
                await BakeSceneGIAsync(token);
                material.TurnOff();
                MoveBakedLightmaps(BakePath.RendererFormat(rendererIndex, i));
            }
        }

        async Task BakeMonitorAsync(int monitorIndex, CancellationToken token)
        {
            var monitor = sceneController.KanikamaMonitors[monitorIndex];
            monitor.OnBake();

            Debug.Log($"Baking Monitor {monitor.Name}");
            var gridCount = monitor.EmissiveMaterials.Count;
            for (var i = 0; i < gridCount; i++)
            {
                Debug.Log($"- Baking Monitor {monitor.Name}'s {i}-th Light");
                var material = monitor.EmissiveMaterials[i];
                material.OnBake();
                Lightmapping.Clear();
                await BakeSceneGIAsync(token);
                material.TurnOff();
                MoveBakedLightmaps(BakePath.MonitorFormat(monitorIndex, i));
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
            MoveBakedLightmaps(BakePath.AmbientFormat());
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
            var bakedLightmapPaths = bakePath.GetUnityLightmapPaths();
            for (var mapIndex = 0; mapIndex < bakedLightmapPaths.Count; mapIndex++)
            {
                var bakedMapPath = bakedLightmapPaths[mapIndex];
                var copiedMapPath = Path.Combine(bakePath.TmpDirPath, string.Format(format, mapIndex));
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

        void DeleteUnusedTextures()
        {
            var lightmapCount = bakePath.GetUnityLightmapPaths().Count;
            var allTexturePaths = bakePath.GetAllTempTexturePaths();

            foreach (var path in allTexturePaths)
            {
                if (path.LightmapIndex >= lightmapCount || !sceneController.ValidateTexturePath(path))
                {
                    Debug.Log($"Delete Unused Texture: {path.Path}");
                    AssetDatabase.DeleteAsset(path.Path);
                }
            }


            var allKanikamaPaths = bakePath.GetAllKanikamaAssetPaths();
            foreach (var path in allKanikamaPaths)
            {
                if (path.LightmapIndex >= lightmapCount)
                {
                    Debug.Log($"Delete Unused Asset: {path.Path}");
                    AssetDatabase.DeleteAsset(path.Path);
                }
            }

            AssetDatabase.Refresh();
        }

        void CreateKanikamaAssets()
        {
            if (!request.isGenerateAssets) return;
            DeleteUnusedTextures();
            var lightMapCount = bakePath.GetUnityLightmapPaths().Count;
            for (var mapIndex = 0; mapIndex < lightMapCount; mapIndex++)
            {
                var textures = bakePath.LoadKanikamaMaps(mapIndex);
                if (!textures.Any()) continue;
                var texArr = Texture2DArrayGenerator.Generate(textures);
                var texArrPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.TexArrFormat, mapIndex));
                AssetUtil.CreateOrReplaceAsset(ref texArr, texArrPath);
                Debug.Log($"Create {texArrPath}");

                var shader = Shader.Find(ShaderName.Composite);
                var mat = new Material(shader);
                var matPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.CompositeMaterialFormat, mapIndex));
                AssetUtil.CreateOrReplaceAsset(ref mat, matPath);
                Debug.Log($"Create {matPath}");

                mat.SetInt(ShaderProperties.TexCount, textures.Count);
                mat.SetTexture(ShaderProperties.TexArray, texArr);

                var sceneMapPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.KanikamaMapFormat, mapIndex));
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
    }
}