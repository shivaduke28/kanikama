using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Editor
{
    public static class BakingPipeline
    {
        [MenuItem("Kanikama/Bake for Kanikama GI", false, 1)]
        public static void Bake()
        {
            if (!KanikamaSceneUtility.TryGetActiveSceneAsset(out var sceneAssetData)) return;
            var sceneDescriptor = Object.FindObjectOfType<KanikamaSceneDescriptor>();
            if (sceneDescriptor == null) return;

            var lightSources = sceneDescriptor.GetLightSources();
            var _ = BakeAsync(sceneAssetData, lightSources, default);
        }

        public static async Task BakeAsync(SceneAssetData sceneAssetData, ILightSourceHandle[] lightSourceHandles, CancellationToken cancellationToken)
        {
            // create a copy of the active scene.
            using (var copiedSceneHandle = KanikamaSceneUtility.CopySceneAsset(sceneAssetData))
            {
                try
                {
                    // open the copied scene
                    EditorSceneManager.OpenScene(copiedSceneHandle.SceneAssetData.Path);

                    // initialize all light source handles **after** opening the copied scene
                    foreach (var lightSourceHandle in lightSourceHandles)
                    {
                        lightSourceHandle.Initialize();
                        lightSourceHandle.TurnOff();
                    }

                    // turn off all light sources but kanikama ones
                    bool Filter(Object obj) => lightSourceHandles.All(l => !l.Includes(obj));
                    var sceneGIContext = KanikamaSceneUtility.GetSceneGIContext(Filter);

                    sceneGIContext.TurnOff();
                    sceneGIContext.DisableLightProbes();
                    sceneGIContext.DisableReflectionProbes();

                    var dstDir = $"{sceneAssetData.LightingAssetDirectoryPath}_kanikama-temp";
                    KanikamaSceneUtility.CreateFolderIfNecessary(dstDir);
                    
                    // TODO: don't use scriptable object here because its lifecyle is unstable when opening a new scene...
                    var bakedAssetRegistry = BakedAssetRegistry.FindOrCreate(Path.Combine(dstDir, BakedAssetRegistry.DefaultFileName));
                    bakedAssetRegistry.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    var lightmapper = new Lightmapper();

                    for (var i = 0; i < lightSourceHandles.Length; i++)
                    {
                        var lightSourceHandle = lightSourceHandles[i];

                        lightSourceHandle.TurnOn();
                        lightmapper.ClearCache();
                        await lightmapper.BakeAsync(cancellationToken);
                        lightSourceHandle.TurnOff();

                        var baked = KanikamaSceneUtility.GetBakedAssetData(copiedSceneHandle.SceneAssetData);
                        var copied = new BakedAssetData();
                        foreach (var bakedLightmap in baked.Lightmaps)
                        {
                            var outPath = Path.Combine(dstDir, TempLightmapName(bakedLightmap, i));
                            var copiedLightmap = KanikamaSceneUtility.CopyBakedLightmap(bakedLightmap, outPath);
                            copied.Lightmaps.Add(copiedLightmap);
                        }
                        foreach (var bakedLightmap in baked.DirectionalLightmaps)
                        {
                            var outPath = Path.Combine(dstDir, TempLightmapName(bakedLightmap, i));
                            var copiedLightmap = KanikamaSceneUtility.CopyBakedLightmap(bakedLightmap, outPath);
                            copied.DirectionalLightmaps.Add(copiedLightmap);
                        }
                        bakedAssetRegistry.AddOrUpdate(i.ToString(), copied);
                    }

                    bakedAssetRegistry.hideFlags = HideFlags.None;
                    EditorUtility.SetDirty(bakedAssetRegistry);
                    AssetDatabase.SaveAssetIfDirty(bakedAssetRegistry);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            EditorSceneManager.OpenScene(sceneAssetData.Path);
        }


        [MenuItem("Kanikama/Create Assets", false, 2)]
        public static void CreateAssets()
        {
            if (!KanikamaSceneUtility.TryGetActiveSceneAsset(out var sceneAssetData)) return;
            var dstDir = $"{sceneAssetData.LightingAssetDirectoryPath}_kanikama-temp";
            KanikamaSceneUtility.CreateFolderIfNecessary(dstDir);

            var bakedAssetRegistry = BakedAssetRegistry.FindOrCreate(Path.Combine(dstDir, BakedAssetRegistry.DefaultFileName));
            CreateAssets(bakedAssetRegistry, $"{sceneAssetData.LightingAssetDirectoryPath}_kanikama-out");
        }

        public static void CreateAssets(BakedAssetRegistry bakedAssetRegistry, string dstDirPath)
        {
            KanikamaSceneUtility.CreateFolderIfNecessary(dstDirPath);

            var bakedDatum = bakedAssetRegistry.GetAllBakedAssetDatum();
            var lightmaps = bakedDatum.SelectMany(data => data.Lightmaps).ToArray();
            var directionalMaps = bakedDatum.SelectMany(data => data.DirectionalLightmaps).ToArray();
            var maxIndex = lightmaps.Max(lightmap => lightmap.Index);
            for (var index = 0; index <= maxIndex; index++)
            {
                var light = lightmaps.Where(l => l.Index == index).Select(l => l.Texture).ToList();
                var dir = directionalMaps.Where(l => l.Index == index).Select(l => l.Texture).ToList();
                var lightArr = KanikamaTextureUtility.CreateTexture2DArray(light, false);
                var dirArr = KanikamaTextureUtility.CreateTexture2DArray(dir, true);
                var lightPath = Path.Combine(dstDirPath, $"{LightmapType.Color.ToFileName()}-{index}.asset");
                var dirPath = Path.Combine(dstDirPath, $"{LightmapType.Directional.ToFileName()}-{index}.asset");
                KanikamaSceneUtility.CreateOrReplaceAsset(ref lightArr, lightPath);
                KanikamaSceneUtility.CreateOrReplaceAsset(ref dirArr, dirPath);
            }
        }

        [MenuItem("Kanikama/Bake non Kanikama GI", false, 0)]
        public static void BakeWithoutKanikama()
        {
            if (!KanikamaSceneUtility.TryGetActiveSceneAsset(out var _)) return;
            var sceneDescriptor = Object.FindObjectOfType<KanikamaSceneDescriptor>();
            if (sceneDescriptor == null) return;

            var lightSources = sceneDescriptor.GetLightSources();
            var _ = BakeWithoutKanikamaAsync(lightSources, default);
        }

        public static async Task BakeWithoutKanikamaAsync(ILightSourceHandle[] lightSourceHandles,
            CancellationToken cancellationToken)
        {
            foreach (var lightSourceHandle in lightSourceHandles)
            {
                lightSourceHandle.Initialize();
                lightSourceHandle.TurnOff();
            }

            var lightmapper = new Lightmapper();
            await lightmapper.BakeAsync(cancellationToken);

            foreach (var lightSourceHandle in lightSourceHandles)
            {
                lightSourceHandle.Dispose();
            }
        }

        static string TempLightmapName(BakedLightmap bakedLightmap, int lightSourceIndex)
        {
            var path = bakedLightmap.Path;
            var fileName = Path.GetFileName(path);
            var ext = Path.GetExtension(fileName);
            return $"{bakedLightmap.Type.ToFileName()}-{bakedLightmap.Index}-{lightSourceIndex}{ext}";
        }
    }
}