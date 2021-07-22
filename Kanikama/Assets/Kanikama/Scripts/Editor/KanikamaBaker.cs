using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Kanikama.EditorOnly;
using UnityEditor;
using System.IO;
using VRC.Udon;
using UdonSharp;
using System.Linq;
using Kanikama.Udon;
using UdonSharpEditor;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace Kanikama.Editor
{
    public class KanikamaBaker
    {
        KanikamaSceneData sceneData;
        int lightmapAtlasCount;
        string bakedAssetsDirPath;
        string exportDirPath;
        string tmpDirPath;

        /// <summary>
        /// key: lightmap index, value: baked kanikama lightmaps
        /// </summary>
        Dictionary<int, List<Texture2D>> kanikamaLightDic = new Dictionary<int, List<Texture2D>>();
        /// <summary>
        /// key: kanikama monitor index, value: (key: lightmap index, value: baked kanikama lightmaps)
        /// </summary>
        Dictionary<int, Dictionary<int, List<Texture2D>>> kanikamaMonitorDic = new Dictionary<int, Dictionary<int, List<Texture2D>>>();


        static readonly Regex LightMapRegex = new Regex("Lightmap-[0-9]+_comp_light.exr");
        const string ExportDirFormat = "{0}_Kanikama";
        const string TmpDirName = "tmp";

        const string LightmapFormat = "Lightmap-{0}_comp_light.exr";
        const string KLmapFormat = "KL-{0}_{1}.exr";
        const string KRmapFormat = "KR-{0}-{1}_{2}.exr";
        const string KAmapFormat = "KA-{0}.exr";

        const string KLTexArrFormat = "KL-array-{0}.asset";
        const string KLCompositeMatFormat = "KL-comp-{0}.mat";
        const string KLCompositeMapFormat = "KL-comp-{0}.asset";

        const string KMmapFormat = "KM-{0}-{1}_{2}.exr";
        const string KMTexArrFormat = "KM-array-{0}-{1}.asset";
        const string KMCompositeMatFormat = "KM-comp-{0}-{1}.mat";
        const string KMCompositeMapFormat = "KM-comp-{0}-{1}.asset";

        const string CompositeShaderName = "Kanikama/LightmapComposite";
        public const string DummyShaderName = "Kanikama/Dummy";
        static readonly int TexCountPropertyId = Shader.PropertyToID("_TexCount");
        static readonly int TexArrayPropertyId = Shader.PropertyToID("_Tex2DArray");

        public async Task BakeAsync(KanikamaSceneDescriptor sceneDescriptor, CancellationToken token = default)
        {
            try
            {
                Debug.Log($"Start to Bake Kanikama.");
                var scene = SceneManager.GetActiveScene();
                sceneData = new KanikamaSceneData(sceneDescriptor);
                sceneData.LoadActiveScene();
                sceneData.TurnOff();

                var sceneDirPath = Path.GetDirectoryName(scene.path);
                var exportDirName = string.Format(ExportDirFormat, scene.name);

                exportDirPath = Path.Combine(sceneDirPath, exportDirName);
                AssetUtil.CreateFolderIfNecessary(exportDirPath, TmpDirName);
                tmpDirPath = Path.Combine(exportDirPath, TmpDirName);
                AssetUtil.CreateFolderIfNecessary(sceneDirPath, exportDirName);

                bakedAssetsDirPath = Path.Combine(sceneDirPath, scene.name.ToLower());

                await BakeKanikamaLightsAsync(token);
                await BakeKanikamaRenderersAsync(token);
                await BakeKanikamaMonitorsAsync(token);
                await BakeKanikamaAmbient(token);
                CreateKanikamaLightAssets();
                CreateKanikamaMonitorAssets();
                AssetDatabase.Refresh();

                sceneData.RollbackNonKanikama();

                Debug.Log($"Baking Scene GI without Kanikama...");
                await BakeSceneGIAsync(token);
                sceneData.RollbackKanikama();

                Debug.Log($"Done.");
            }
            catch (TaskCanceledException e)
            {
                throw e;
            }
            finally
            {
                sceneData?.Dispose();
            }
        }

        async Task BakeKanikamaLightsAsync(CancellationToken token)
        {
            var lightCount = sceneData.kanikamaLightDatas.Count;
            if (lightCount == 0) return;

            for (var i = 0; i < lightCount; i++)
            {
                Debug.Log($"Baking Kanikama Light... ({i + 1}/{lightCount})");
                var light = sceneData.kanikamaLightDatas[i];
                light.Enabled = true;
                // may throw TaskCancelledException
                await BakeSceneGIAsync(token);
                light.Enabled = false;
                MoveBakedKanikamaLights(i);
            }
        }

        void MoveBakedKanikamaLights(int lightIndex)
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

        void CreateKanikamaLightAssets()
        {
            if (!kanikamaLightDic.Any()) return;

            var texCount = sceneData.kanikamaLightDatas.Count;
            texCount += sceneData.kanikamaRendererDatas.SelectMany(x => x.EmissiveMaterialDatas).Count();
            texCount += sceneData.IsKanikamaAmbientEnable ? 1 : 0;

            for (var mapIndex = 0; mapIndex < lightmapAtlasCount; mapIndex++)
            {
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


        async Task BakeKanikamaRenderersAsync(CancellationToken token)
        {
            var rendererCount = sceneData.kanikamaRendererDatas.Count;
            if (rendererCount == 0) return;

            for (var i = 0; i < rendererCount; i++)
            {
                Debug.Log($"Baking Kanikama Renderer... ({i + 1}/{rendererCount})");
                var rendererData = sceneData.kanikamaRendererDatas[i];

                var matCount = rendererData.EmissiveMaterialDatas.Count;
                for (var j = 0; j < matCount; j++)
                {
                    Debug.Log($"- Baking Kanikama Renderer {i + 1} Material Light... ({i + 1}/{rendererCount})");
                    var matData = rendererData.EmissiveMaterialDatas[j];
                    matData.OnBake();
                    // may throw TaskCancelledException
                    await BakeSceneGIAsync(token);
                    matData.TurnOff();
                    MoveBakedKanikamaRenderers(i, j);
                }
            }
        }

        void MoveBakedKanikamaRenderers(int rendererIndex, int materialIndex)
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

        async Task BakeKanikamaMonitorsAsync(CancellationToken token)
        {
            kanikamaMonitorDic.Clear();

            var monitorCount = sceneData.kanikamaMonitors.Count;
            if (monitorCount == 0) return;

            for (var m = 0; m < sceneData.kanikamaMonitors.Count; m++)
            {
                Debug.Log($"Baking Kanikama Monitor... ({m + 1}/{monitorCount})");
                kanikamaMonitorDic[m] = new Dictionary<int, List<Texture2D>>();
                var monitor = sceneData.kanikamaMonitors[m];
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

        void MoveBakedKanikamaMonitors(int monitorIndex, int lightIndex)
        {
            if (lightmapAtlasCount == 0) lightmapAtlasCount = GetBakedLightmapCount(bakedAssetsDirPath);

            var textureDic = new Dictionary<int, List<Texture2D>>();
            kanikamaMonitorDic[monitorIndex] = textureDic;

            for (var mapIndex = 0; mapIndex < lightmapAtlasCount; mapIndex++)
            {
                var bakedMapPath = Path.Combine(bakedAssetsDirPath, string.Format(LightmapFormat, mapIndex));
                var copiedMapPath = Path.Combine(tmpDirPath, string.Format(KMmapFormat, monitorIndex, mapIndex, lightIndex));
                AssetDatabase.CopyAsset(bakedMapPath, copiedMapPath);

                var textureImporter = AssetImporter.GetAtPath(copiedMapPath) as TextureImporter;
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();

                if (!textureDic.ContainsKey(mapIndex))
                {
                    textureDic[mapIndex] = new List<Texture2D>();
                }

                textureDic[mapIndex].Add(AssetDatabase.LoadAssetAtPath<Texture2D>(copiedMapPath));
            }
        }

        void CreateKanikamaMonitorAssets()
        {
            if (!kanikamaMonitorDic.Any()) return;
            for (var m = 0; m < sceneData.kanikamaMonitors.Count; m++)
            {
                for (var mapIndex = 0; mapIndex < lightmapAtlasCount; mapIndex++)
                {

                    var texArr = Texture2DArrayGenerator.Generate(kanikamaMonitorDic[m][mapIndex]);
                    AssetUtil.CreateOrReplaceAsset(ref texArr, Path.Combine(exportDirPath, string.Format(KMTexArrFormat, m, mapIndex)));

                    var shader = Shader.Find(CompositeShaderName);
                    var mat = new Material(shader);
                    var matPath = Path.Combine(exportDirPath, string.Format(KMCompositeMatFormat, m, mapIndex));
                    AssetUtil.CreateOrReplaceAsset(ref mat, matPath);

                    mat.SetInt(TexCountPropertyId, sceneData.kanikamaMonitors[m].Lights.Count);
                    mat.SetTexture(TexArrayPropertyId, texArr);

                    var crtPath = Path.Combine(exportDirPath, string.Format(KMCompositeMapFormat, m, mapIndex));
                    var sceneMap = new CustomRenderTexture(texArr.width, texArr.height, RenderTextureFormat.ARGBHalf)
                    {
                        material = mat,
                        updateMode = CustomRenderTextureUpdateMode.Realtime,
                        initializationMode = CustomRenderTextureUpdateMode.OnLoad,
                        initializationColor = Color.black
                    };
                    AssetUtil.CreateOrReplaceAsset(ref sceneMap, crtPath);
                }
            }

            AssetDatabase.Refresh();
        }



        async Task BakeKanikamaAmbient(CancellationToken token)
        {
            if (!sceneData.IsKanikamaAmbientEnable) return;

            Debug.Log($"Baking Kanikama Ambient...");

            sceneData.OnAmbientBake();
            await BakeSceneGIAsync(token);
            sceneData.TurnOffAmbient();

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
                    Lightmapping.ForceStop();
                    throw e;
                }
            }
        }


        static int GetBakedLightmapCount(string bakedAssetsDirPath)
        {
            return AssetDatabase.FindAssets("t:Texture", new string[1] { bakedAssetsDirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => LightMapRegex.IsMatch(x)).Count();
        }
    }
}