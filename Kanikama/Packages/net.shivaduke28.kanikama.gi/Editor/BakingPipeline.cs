using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core;
using Kanikama.Core.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Editor
{
    public static class BakingPipeline
    {
        public sealed class BakingContext
        {
            public BakingConfiguration BakingConfiguration;
            public SceneAssetData SceneAssetData;
        }

        public static async Task BakeAsync(BakingContext context, CancellationToken cancellationToken)
        {
            using (var copiedSceneHandle = KanikamaSceneUtility.CopySceneAsset(context.SceneAssetData))
            {
                try
                {
                    // open the copied scene
                    EditorSceneManager.OpenScene(copiedSceneHandle.SceneAssetData.Path);

                    // initialize all light source handles **after** the copied scene is opened
                    var config = context.BakingConfiguration;
                    config.UpdateSceneAsset(copiedSceneHandle.SceneAssetData.Asset);

                    var lightSourceHandles = config.GetLightSources().Select(id => new ObjectHandle<LightSource>(id)).ToArray();
                    var lightSourceGroupHandles = config.GetLightSourceGroups().Select(id => new ObjectHandle<LightSourceGroup>(id)).ToArray();

                    foreach (var lightSourceHandle in lightSourceHandles)
                    {
                        lightSourceHandle.Value.Initialize();
                        lightSourceHandle.Value.TurnOff();
                    }

                    foreach (var lightSourceGroupHandle in lightSourceGroupHandles)
                    {
                        foreach (var lightSource in lightSourceGroupHandle.Value.GetLightSources())
                        {
                            lightSource.Initialize();
                            lightSource.TurnOff();
                        }
                    }

                    // save scene
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

                    // turn off all light sources but kanikama ones
                    bool Filter(Object obj) =>
                        lightSourceHandles.All(l => !l.Value.Includes(obj)) &&
                        lightSourceGroupHandles.All(g => !g.Value.Includes(obj));

                    var sceneGIContext = KanikamaSceneUtility.GetSceneGIContext(Filter);

                    sceneGIContext.TurnOff();
                    sceneGIContext.DisableLightProbes();
                    sceneGIContext.DisableReflectionProbes();

                    var dstDir = $"{context.SceneAssetData.LightingAssetDirectoryPath}_kanikama-temp";
                    KanikamaSceneUtility.CreateFolderIfNecessary(dstDir);

                    var bakedAssetDataBase = new BakedAssetDataBase();
                    var lightmapper = new Lightmapper();

                    for (var i = 0; i < lightSourceHandles.Length; i++)
                    {
                        var lightSourceHandle = lightSourceHandles[i];

                        lightSourceHandle.Value.TurnOn();
                        lightmapper.ClearCache();
                        await lightmapper.BakeAsync(cancellationToken);
                        lightSourceHandle.Value.TurnOff();

                        var baked = KanikamaSceneUtility.GetBakedAssetData(copiedSceneHandle.SceneAssetData);
                        var copied = new BakedLightingAssetCollection();
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

                        // TODO: use "id" of lightsource
                        bakedAssetDataBase.AddOrUpdate(i.ToString(), copied);
                    }

                    // TODO: bake LightSourceGroup

                    var repository = BakedAssetRepository.FindOrCreate(Path.Combine(dstDir, BakedAssetRepository.DefaultFileName));
                    repository.DataBase = bakedAssetDataBase;
                    EditorUtility.SetDirty(repository);
                    AssetDatabase.SaveAssets();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    KanikamaDebug.LogException(e);
                }
            }

            EditorSceneManager.OpenScene(context.SceneAssetData.Path);
        }


        [MenuItem("Kanikama/Create Assets", false, 2)]
        public static void CreateAssets()
        {
            if (!KanikamaSceneUtility.TryGetActiveSceneAsset(out var sceneAssetData)) return;
            var dstDir = $"{sceneAssetData.LightingAssetDirectoryPath}_kanikama-temp";
            KanikamaSceneUtility.CreateFolderIfNecessary(dstDir);

            var bakedAssetRegistry = BakedAssetRepository.FindOrCreate(Path.Combine(dstDir, BakedAssetRepository.DefaultFileName));
            CreateAssets(bakedAssetRegistry.DataBase, $"{sceneAssetData.LightingAssetDirectoryPath}_kanikama-out");
            EditorUtility.SetDirty(bakedAssetRegistry);
            AssetDatabase.SaveAssets();
        }

        public static void CreateAssets(BakedAssetDataBase bakedAssetDataBase, string dstDirPath)
        {
            KanikamaSceneUtility.CreateFolderIfNecessary(dstDirPath);

            var bakedDatum = bakedAssetDataBase.GetAllBakedAssets();
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

        public static async Task BakeWithoutKanikamaAsync(BakingContext context, CancellationToken cancellationToken)
        {
            var config = context.BakingConfiguration;
            var lightSourceHandles = config.GetLightSources().Select(id => new ObjectHandle<LightSource>(id)).ToArray();
            var lightSourceGroupHandles = config.GetLightSourceGroups().Select(id => new ObjectHandle<LightSourceGroup>(id)).ToArray();

            foreach (var lightSourceHandle in lightSourceHandles)
            {
                lightSourceHandle.Value.Initialize();
                lightSourceHandle.Value.TurnOff();
            }

            foreach (var lightSourceGroupHandle in lightSourceGroupHandles)
            {
                foreach (var lightSource in lightSourceGroupHandle.Value.GetLightSources())
                {
                    lightSource.Initialize();
                    lightSource.TurnOff();
                }
            }

            try
            {
                var lightmapper = new Lightmapper();
                lightmapper.ClearCache();
                await lightmapper.BakeAsync(cancellationToken);
            }
            catch (Exception e)
            {
                KanikamaDebug.LogException(e);
            }
            finally
            {
                foreach (var lightSourceHandle in lightSourceHandles)
                {
                    lightSourceHandle.Value.Clear();
                }

                foreach (var lightSourceGroupHandle in lightSourceGroupHandles)
                {
                    foreach (var lightSource in lightSourceGroupHandle.Value.GetLightSources())
                    {
                        lightSource.Clear();
                    }
                }
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
