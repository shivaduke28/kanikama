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
            public const string CompositeCRT = "Kanikama/Composite/CRT";
            public const string CompositeUnlit = "Kanikama/Composite/Unlit";
            public const string Dummy = "Kanikama/Dummy";
        }

        public static class ShaderProperties
        {
            public static readonly int LighmapCount = Shader.PropertyToID("_LightmapCount");
            public static readonly int LightmapArray = Shader.PropertyToID("_LightmapArray");
            public static readonly int Lightmap = Shader.PropertyToID("_Lightmap");
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
                sceneController.SetLightmapSettings(request.isDirectionalMode);
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
            Lightmapping.Clear();
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
            var gridCount = monitor.MaterialGroups.Count;
            for (var i = 0; i < gridCount; i++)
            {
                Debug.Log($"- Baking Monitor {monitor.Name}'s {i}-th Light");
                var material = monitor.MaterialGroups[i];
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
            Lightmapping.Clear();
            await BakeSceneGIAsync(token);
            sceneController.TurnOffAmbient();
            MoveBakedLightmaps(BakePath.AmbientFormat());
        }

        async Task BakeWithoutKanikamaAsync(CancellationToken token)
        {
            if (!request.IsBakeWithouKanikama()) return;
            sceneController.RollbackNonKanikama();
            sceneController.RollbackLightmapSettings();
            Debug.Log($"Baking Scene GI without Kanikama...");
            await BakeSceneGIAsync(token);
        }

        void MoveBakedLightmaps(string format)
        {
            var bakedLightmapPaths = bakePath.GetUnityLightmapPaths();
            for (var mapIndex = 0; mapIndex < bakedLightmapPaths.Count; mapIndex++)
            {
                var bakedMapPath = bakedLightmapPaths[mapIndex];
                var copiedFileName = string.Format(format, mapIndex);
                var copiedMapPath = Path.Combine(bakePath.TmpDirPath, copiedFileName);
                AssetDatabase.CopyAsset(bakedMapPath, copiedMapPath);

                var textureImporter = AssetImporter.GetAtPath(copiedMapPath) as TextureImporter;
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();
            }

            if (request.isDirectionalMode)
            {
                var bakedDirectionalLightmapPaths = bakePath.GetUnityDirectionalLightmapPaths();
                for (var mapIndex = 0; mapIndex < bakedLightmapPaths.Count; mapIndex++)
                {
                    var bakedDirMapPath = bakedDirectionalLightmapPaths[mapIndex];
                    var dirFileName = $"{BakePath.DirectionalPrefix}{Path.GetFileNameWithoutExtension(string.Format(format, mapIndex))}.png";
                    var copiedDirMapPath = Path.Combine(bakePath.TmpDirPath, dirFileName);
                    AssetDatabase.CopyAsset(bakedDirMapPath, copiedDirMapPath);
                    var textureImporter = AssetImporter.GetAtPath(copiedDirMapPath) as TextureImporter;
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();
                }
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

        //void DeleteUnusedTextures()
        //{
        //    var lightmapCount = bakePath.GetUnityLightmapPaths().Count;
        //    var allTexturePaths = bakePath.GetAllTempTexturePaths();

        //    foreach (var path in allTexturePaths)
        //    {
        //        if (path.LightmapIndex >= lightmapCount || !sceneController.ValidateTexturePath(path))
        //        {
        //            Debug.Log($"Delete Unused Texture: {path.Path}");
        //            AssetDatabase.DeleteAsset(path.Path);
        //        }
        //    }

        //    var allKanikamaPaths = bakePath.GetAllKanikamaAssetPaths();
        //    foreach (var path in allKanikamaPaths)
        //    {
        //        if (path.LightmapIndex >= lightmapCount)
        //        {
        //            Debug.Log($"Delete Unused Asset: {path.Path}");
        //            AssetDatabase.DeleteAsset(path.Path);
        //        }
        //    }

        //    AssetDatabase.Refresh();
        //}

        void CreateKanikamaAssets()
        {
            if (!request.isGenerateAssets) return;

            var lightMapCount = bakePath.GetUnityLightmapPaths().Count;
            for (var mapIndex = 0; mapIndex < lightMapCount; mapIndex++)
            {
                var textures = bakePath.GetKanikamaMapPaths(mapIndex)
                    .Where(path => sceneController.ValidateTexturePath(path))
                    .Select(x => AssetDatabase.LoadAssetAtPath<Texture2D>(x.Path))
                    .ToList();
                if (!textures.Any()) continue;
                var texArr = Texture2DArrayGenerator.Generate(textures);
                var texArrPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.TexArrFormat, mapIndex));
                AssetUtil.CreateOrReplaceAsset(ref texArr, texArrPath);
                Debug.Log($"Create {texArrPath}");

                if (request.createRenderTexture)
                {
                    CreateRTAssets(mapIndex, texArr);
                }
                if (request.createCustomRenderTexture)
                {
                    CreateCRTAssets(mapIndex, texArr);
                }
                if (request.isDirectionalMode)
                {
                    CreateDirectionalMapArray(mapIndex);
                }
            }
            AssetDatabase.Refresh();
        }

        void CreateDirectionalMapArray(int mapIndex)
        {
            var dirTextures = bakePath.GetKanikamaDirectionalMapPaths(mapIndex)
                .Where(x => sceneController.ValidateTexturePath(x))
                .Select(x => AssetDatabase.LoadAssetAtPath<Texture2D>(x.Path))
                .ToList();
            var dirTexArr = Texture2DArrayGenerator.Generate(dirTextures);
            var dirTexArrPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.DirTexArrFormat, mapIndex));
            AssetUtil.CreateOrReplaceAsset(ref dirTexArr, dirTexArrPath);
            Debug.Log($"Create {dirTexArrPath}");
        }

        void CreateCRTAssets(int mapIndex, Texture2DArray texture2DArray)
        {
            var shader = Shader.Find(ShaderName.CompositeCRT);
            var mat = new Material(shader);
            var matPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.CustomRenderTextureMaterialFormat, mapIndex));
            AssetUtil.CreateOrReplaceAsset(ref mat, matPath);
            Debug.Log($"Create {matPath}");

            mat.SetInt(ShaderProperties.LighmapCount, texture2DArray.depth);
            mat.SetTexture(ShaderProperties.LightmapArray, texture2DArray);

            var sceneMapPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.CustomRenderTextureFormat, mapIndex));
            var sceneMap = new CustomRenderTexture(texture2DArray.width, texture2DArray.height, RenderTextureFormat.ARGBHalf)
            {
                useMipMap = true,
                material = mat,
                updateMode = CustomRenderTextureUpdateMode.Realtime,
                initializationMode = CustomRenderTextureUpdateMode.OnLoad,
                initializationColor = Color.black
            };
            AssetUtil.CreateOrReplaceAsset(ref sceneMap, sceneMapPath);
            Debug.Log($"Create {sceneMapPath}");
        }

        void CreateRTAssets(int mapIndex, Texture2DArray texture2DArray)
        {
            var shader = Shader.Find(ShaderName.CompositeUnlit);
            var mat = new Material(shader);
            var matPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.RenderTextureMaterialFormat, mapIndex));
            AssetUtil.CreateOrReplaceAsset(ref mat, matPath);
            Debug.Log($"Create {matPath}");

            mat.SetInt(ShaderProperties.LighmapCount, texture2DArray.depth);
            mat.SetTexture(ShaderProperties.LightmapArray, texture2DArray);

            var sceneMapPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.RenderTextureFormat, mapIndex));
            var sceneMap = new RenderTexture(texture2DArray.width, texture2DArray.height, 0, RenderTextureFormat.ARGBHalf)
            {
                useMipMap = true
            };
            AssetUtil.CreateOrReplaceAsset(ref sceneMap, sceneMapPath);
            Debug.Log($"Create {sceneMapPath}");
        }
    }
}