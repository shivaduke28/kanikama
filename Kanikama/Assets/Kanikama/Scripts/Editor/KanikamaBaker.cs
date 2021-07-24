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
using Kanikama.EditorOnly;

namespace Kanikama.Editor
{
    public class KanikamaBaker
    {
        private KanikamaSceneManager sceneManager;
        private int lightmapAtlasCount;
        private string bakedAssetsDirPath;
        private string exportDirPath;
        private string tmpDirPath;

        private readonly Dictionary<int, List<Texture2D>> kanikamaLightDic = new Dictionary<int, List<Texture2D>>();


        // Unity lightmapper Constants
        private static readonly Regex LightMapRegex = new Regex("Lightmap-[0-9]+_comp_light.exr");
        private const string LightmapFormat = "Lightmap-{0}_comp_light.exr";

        // Kanikama export settings
        private const string ExportDirFormat = "{0}_Kanikama";
        private const string TmpDirName = "tmp";

        private const string KLmapFormat = "KL-{0}_{1}.exr";
        private const string KRmapFormat = "KR-{0}-{1}_{2}.exr";
        private const string KAmapFormat = "KA-{0}.exr";
        private const string KMmapFormat = "KM-{0}-{1}_{2}.exr";

        private const string KLTexArrFormat = "KL-array-{0}.asset";
        private const string KLCompositeMatFormat = "KL-mat-comp-{0}.mat";
        private const string KLCompositeMapFormat = "KL-comp-{0}.asset";

        public const string CompositeShaderName = "Kanikama/Composite";
        public const string DummyShaderName = "Kanikama/Dummy";

        private static readonly int TexCountPropertyId = Shader.PropertyToID("_TexCount");
        private static readonly int TexArrayPropertyId = Shader.PropertyToID("_Tex2DArray");

        public async Task BakeAsync(KanikamaSceneDescriptor sceneDescriptor, CancellationToken token = default)
        {
            try
            {
                Debug.Log($"Start to Bake Kanikama.");
                var scene = SceneManager.GetActiveScene();
                sceneManager = new KanikamaSceneManager(sceneDescriptor);
                sceneManager.LoadActiveScene();
                sceneManager.TurnOff();

                var sceneDirPath = Path.GetDirectoryName(scene.path);
                var exportDirName = string.Format(ExportDirFormat, scene.name);

                AssetUtil.CreateFolderIfNecessary(sceneDirPath, exportDirName);
                exportDirPath = Path.Combine(sceneDirPath, exportDirName);
                AssetUtil.CreateFolderIfNecessary(exportDirPath, TmpDirName);
                tmpDirPath = Path.Combine(exportDirPath, TmpDirName);

                bakedAssetsDirPath = Path.Combine(sceneDirPath, scene.name.ToLower());

                await BakeKanikamaLightsAsync(token);
                await BakeKanikamaRenderersAsync(token);
                await BakeKanikamaMonitorsAsync(token);
                await BakeKanikamaAmbient(token);
                CreateKanikamaLightAssets();
                //CreateKanikamaMonitorAssets();
                AssetDatabase.Refresh();

                sceneManager.RollbackNonKanikama();

                Debug.Log($"Baking Scene GI without Kanikama...");
                await BakeSceneGIAsync(token);
                sceneManager.RollbackKanikama();

                Debug.Log($"Done.");
            }
            catch (TaskCanceledException e)
            {
                throw e;
            }
            finally
            {
                sceneManager?.Dispose();
            }
        }

        private async Task BakeKanikamaLightsAsync(CancellationToken token)
        {
            var lightCount = sceneManager.KanikamaLights.Count;
            if (lightCount == 0) return;

            for (var i = 0; i < lightCount; i++)
            {
                Debug.Log($"Baking Kanikama Light... ({i + 1}/{lightCount})");
                var light = sceneManager.KanikamaLights[i];
                light.OnBake();
                // may throw TaskCancelledException
                await BakeSceneGIAsync(token);
                light.TurnOff();
                MoveBakedKanikamaLights(i);
            }
        }

        private void MoveBakedKanikamaLights(int lightIndex)
        {
            if (lightmapAtlasCount == 0) lightmapAtlasCount = GetBakedLightmapCount(bakedAssetsDirPath);

            for (var mapIndex = 0; mapIndex < lightmapAtlasCount; mapIndex++)
            {
                var bakedMapPath = Path.Combine(bakedAssetsDirPath, string.Format(LightmapFormat, mapIndex));
                var copiedMapPath = Path.Combine(tmpDirPath, string.Format(KLmapFormat, mapIndex, lightIndex));
                AssetDatabase.CopyAsset(bakedMapPath, copiedMapPath);

                var textureImporter = AssetImporter.GetAtPath(copiedMapPath) as TextureImporter;
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();

                if (!kanikamaLightDic.ContainsKey(mapIndex))
                {
                    kanikamaLightDic[mapIndex] = new List<Texture2D>();
                }

                kanikamaLightDic[mapIndex].Add(AssetDatabase.LoadAssetAtPath<Texture2D>(copiedMapPath));
            }
        }

        private void CreateKanikamaLightAssets()
        {
            if (!kanikamaLightDic.Any()) return;

            for (var mapIndex = 0; mapIndex < lightmapAtlasCount; mapIndex++)
            {
                var texCount = kanikamaLightDic[mapIndex].Count;
                var texArr = Texture2DArrayGenerator.Generate(kanikamaLightDic[mapIndex]);
                AssetUtil.CreateOrReplaceAsset(ref texArr, Path.Combine(exportDirPath, string.Format(KLTexArrFormat, mapIndex)));

                var shader = Shader.Find(CompositeShaderName);
                var mat = new Material(shader);
                var matPath = Path.Combine(exportDirPath, string.Format(KLCompositeMatFormat, mapIndex));
                AssetUtil.CreateOrReplaceAsset(ref mat, matPath);

                mat.SetInt(TexCountPropertyId, texCount);
                mat.SetTexture(TexArrayPropertyId, texArr);

                var sceneMapPath = Path.Combine(exportDirPath, string.Format(KLCompositeMapFormat, mapIndex));
                var sceneMap = new CustomRenderTexture(texArr.width, texArr.height, RenderTextureFormat.ARGBHalf)
                {
                    material = mat,
                    updateMode = CustomRenderTextureUpdateMode.Realtime,
                    initializationMode = CustomRenderTextureUpdateMode.OnLoad,
                    initializationColor = Color.black
                };
                AssetUtil.CreateOrReplaceAsset(ref sceneMap, sceneMapPath);
            }
            AssetDatabase.Refresh();
        }

        private async Task BakeKanikamaRenderersAsync(CancellationToken token)
        {
            var rendererCount = sceneManager.KanikamaEmissiveRenderers.Count;
            if (rendererCount == 0) return;

            for (var i = 0; i < rendererCount; i++)
            {
                Debug.Log($"Baking Kanikama Renderer... ({i + 1}/{rendererCount})");
                var renderer = sceneManager.KanikamaEmissiveRenderers[i];

                var matCount = renderer.EmissiveMaterial.Count;
                for (var j = 0; j < matCount; j++)
                {
                    Debug.Log($"- Baking Kanikama Renderer {i + 1} Material Light... ({i + 1}/{rendererCount})");
                    var material = renderer.EmissiveMaterial[j];
                    material.OnBake();
                    // may throw TaskCancelledException
                    await BakeSceneGIAsync(token);
                    material.TurnOff();
                    MoveBakedKanikamaRenderers(i, j);
                }
            }
        }

        private void MoveBakedKanikamaRenderers(int rendererIndex, int materialIndex)
        {
            if (lightmapAtlasCount == 0) lightmapAtlasCount = GetBakedLightmapCount(bakedAssetsDirPath);

            for (var mapIndex = 0; mapIndex < lightmapAtlasCount; mapIndex++)
            {
                var bakedMapPath = Path.Combine(bakedAssetsDirPath, string.Format(LightmapFormat, mapIndex));
                var copiedMapPath = Path.Combine(tmpDirPath, string.Format(KRmapFormat, mapIndex, rendererIndex, materialIndex));
                AssetDatabase.CopyAsset(bakedMapPath, copiedMapPath);

                var textureImporter = AssetImporter.GetAtPath(copiedMapPath) as TextureImporter;
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();

                if (!kanikamaLightDic.ContainsKey(mapIndex))
                {
                    kanikamaLightDic[mapIndex] = new List<Texture2D>();
                }

                kanikamaLightDic[mapIndex].Add(AssetDatabase.LoadAssetAtPath<Texture2D>(copiedMapPath));
            }
        }

        private async Task BakeKanikamaMonitorsAsync(CancellationToken token)
        {
            var monitorCount = sceneManager.KanikamaMonitors.Count;
            if (monitorCount == 0) return;

            for (var m = 0; m < sceneManager.KanikamaMonitors.Count; m++)
            {
                Debug.Log($"Baking Kanikama Monitor... ({m + 1}/{monitorCount})");
                var monitor = sceneManager.KanikamaMonitors[m];
                var lightCount = monitor.Lights.Count;
                for (var i = 0; i < lightCount; i++)
                {
                    Debug.Log($"- Baking Kanikama Monitor {m + 1} Light... ({i + 1}/{lightCount})");
                    var light = monitor.Lights[i];
                    light.enabled = true;
                    // may throw TaskCanceldException
                    await BakeSceneGIAsync(token);
                    light.enabled = false;
                    MoveBakedKanikamaMonitors(m, i);
                }
            }
        }

        private void MoveBakedKanikamaMonitors(int monitorIndex, int lightIndex)
        {
            if (lightmapAtlasCount == 0) lightmapAtlasCount = GetBakedLightmapCount(bakedAssetsDirPath);

            for (var mapIndex = 0; mapIndex < lightmapAtlasCount; mapIndex++)
            {
                var bakedMapPath = Path.Combine(bakedAssetsDirPath, string.Format(LightmapFormat, mapIndex));
                var copiedMapPath = Path.Combine(tmpDirPath, string.Format(KMmapFormat, monitorIndex, mapIndex, lightIndex));
                AssetDatabase.CopyAsset(bakedMapPath, copiedMapPath);

                var textureImporter = AssetImporter.GetAtPath(copiedMapPath) as TextureImporter;
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();

                if (!kanikamaLightDic.ContainsKey(mapIndex))
                {
                    kanikamaLightDic[mapIndex] = new List<Texture2D>();
                }

                kanikamaLightDic[mapIndex].Add(AssetDatabase.LoadAssetAtPath<Texture2D>(copiedMapPath));
            }
        }

        private async Task BakeKanikamaAmbient(CancellationToken token)
        {
            if (!sceneManager.IsKanikamaAmbientEnable) return;

            Debug.Log($"Baking Kanikama Ambient...");

            sceneManager.OnAmbientBake();
            await BakeSceneGIAsync(token);
            sceneManager.TurnOffAmbient();

            if (lightmapAtlasCount == 0) lightmapAtlasCount = GetBakedLightmapCount(bakedAssetsDirPath);

            for (var mapIndex = 0; mapIndex < lightmapAtlasCount; mapIndex++)
            {
                var bakedMapPath = Path.Combine(bakedAssetsDirPath, string.Format(LightmapFormat, mapIndex));
                var copiedMapPath = Path.Combine(tmpDirPath, string.Format(KAmapFormat, mapIndex));
                AssetDatabase.CopyAsset(bakedMapPath, copiedMapPath);

                var textureImporter = AssetImporter.GetAtPath(copiedMapPath) as TextureImporter;
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();

                if (!kanikamaLightDic.ContainsKey(mapIndex))
                {
                    kanikamaLightDic[mapIndex] = new List<Texture2D>();
                }

                kanikamaLightDic[mapIndex].Add(AssetDatabase.LoadAssetAtPath<Texture2D>(copiedMapPath));
            }
        }

        private static async Task BakeSceneGIAsync(CancellationToken token)
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
                    Lightmapping.ForceStop();
                    throw e;
                }
            }
        }

        private static int GetBakedLightmapCount(string bakedAssetsDirPath)
        {
            return AssetDatabase.FindAssets("t:Texture", new string[1] { bakedAssetsDirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => LightMapRegex.IsMatch(x)).Count();
        }
    }
}