using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core;
using Kanikama.Core.Editor;
using Kanikama.Core.Editor.Textures;
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
            public List<IBakeableHandle> BakeableHandles;
        }

        public static async Task BakeAsync(BakingContext context, CancellationToken cancellationToken)
        {
            using (var copiedSceneHandle = KanikamaSceneUtility.CopySceneAsset(context.SceneAssetData))
            {
                try
                {
                    // open the copied scene
                    EditorSceneManager.OpenScene(copiedSceneHandle.SceneAssetData.Path);

                    var bakeableHandles = context.BakeableHandles;
                    // TODO: get handles from groups
                    // var lightSourceGroupHandles = config.GetLightSourceGroups().Select(id => new ObjectHandle<LightSourceGroup>(id)).ToArray();

                    var guid = AssetDatabase.AssetPathToGUID(copiedSceneHandle.SceneAssetData.Path);

                    // initialize all light source handles **after** the copied scene is opened
                    foreach (var handle in bakeableHandles)
                    {
                        handle.ReplaceSceneGuid(guid);
                        handle.Initialize();
                        handle.TurnOff();
                    }

                    // foreach (var lightSourceGroupHandle in lightSourceGroupHandles)
                    // {
                    //     foreach (var lightSource in lightSourceGroupHandle.Value.GetLightSources())
                    //     {
                    //         lightSource.Initialize();
                    //         lightSource.TurnOff();
                    //     }
                    // }

                    // save scene
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

                    // turn off all light sources but kanikama ones
                    bool Filter(Object obj) =>
                        bakeableHandles.All(l => !l.Includes(obj)); // &&
                    // lightSourceGroupHandles.All(g => !g.Value.Includes(obj));

                    var sceneGIContext = KanikamaSceneUtility.GetSceneGIContext(Filter);

                    sceneGIContext.TurnOff();
                    sceneGIContext.DisableLightProbes();
                    sceneGIContext.DisableReflectionProbes();

                    var dstDir = $"{context.SceneAssetData.LightingAssetDirectoryPath}_kanikama-temp";
                    KanikamaSceneUtility.CreateFolderIfNecessary(dstDir);

                    var bakedAssetDataBase = new BakedAssetDataBase();
                    var lightmapper = new Lightmapper();

                    foreach (var handle in bakeableHandles)
                    {
                        handle.TurnOn();
                        lightmapper.ClearCache();
                        await lightmapper.BakeAsync(cancellationToken);
                        handle.TurnOff();

                        var baked = KanikamaSceneUtility.GetBakedAssetData(copiedSceneHandle.SceneAssetData);
                        var copied = new BakedLightingAssetCollection();
                        CopyBakedLightingAssetCollection(baked, copied, dstDir, handle.Id);

                        bakedAssetDataBase.AddOrUpdate(handle.Id, copied);
                    }

                    // foreach (var lightSourceGroupHandle in lightSourceGroupHandles)
                    // {
                    //     for (var i = 0; i < lightSourceGroupHandle.Value.GetLightSources().Count; i++)
                    //     {
                    //         // NOTE: need to access to ILightSource via ObjectHandle
                    //         // because LightSourceGroup may be destroyed when baking...
                    //         lightSourceGroupHandle.Value.GetLightSources()[i].TurnOn();
                    //         lightmapper.ClearCache();
                    //         await lightmapper.BakeAsync(cancellationToken);
                    //         lightSourceGroupHandle.Value.GetLightSources()[i].TurnOff();
                    //
                    //         var baked = KanikamaSceneUtility.GetBakedAssetData(copiedSceneHandle.SceneAssetData);
                    //         var copied = new BakedLightingAssetCollection();
                    //         var id = $"{lightSourceGroupHandle.LocalFileId()}_{i}";
                    //         CopyBakedLightingAssetCollection(baked, copied, dstDir, id);
                    //
                    //         bakedAssetDataBase.AddOrUpdate(id, copied);
                    //     }
                    // }

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

        static void CopyBakedLightingAssetCollection(BakedLightingAssetCollection src, BakedLightingAssetCollection dst, string dstDir,
            string id)
        {
            foreach (var bakedLightmap in src.Lightmaps)
            {
                var outPath = Path.Combine(dstDir, TempLightmapName(bakedLightmap, id));
                var copiedLightmap = KanikamaSceneUtility.CopyBakedLightmap(bakedLightmap, outPath);
                dst.Lightmaps.Add(copiedLightmap);
            }

            foreach (var bakedLightmap in src.DirectionalLightmaps)
            {
                var outPath = Path.Combine(dstDir, TempLightmapName(bakedLightmap, id));
                var copiedLightmap = KanikamaSceneUtility.CopyBakedLightmap(bakedLightmap, outPath);
                dst.DirectionalLightmaps.Add(copiedLightmap);
            }
        }

        public static void CreateAssets(BakedAssetDataBase bakedAssetDataBase, string dstDirPath, TextureResizeType resizeType)
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

                foreach (var texture in light)
                {
                    TextureUtility.ResizeTexture(texture, resizeType);
                }

                foreach (var texture in dir)
                {
                    TextureUtility.ResizeTexture(texture, resizeType);
                }

                var lightArr = TextureUtility.CreateTexture2DArray(light, false);
                var dirArr = TextureUtility.CreateTexture2DArray(dir, true);
                var lightPath = Path.Combine(dstDirPath, $"{LightmapType.Color.ToFileName()}-{index}.asset");
                var dirPath = Path.Combine(dstDirPath, $"{LightmapType.Directional.ToFileName()}-{index}.asset");
                KanikamaSceneUtility.CreateOrReplaceAsset(ref lightArr, lightPath);
                KanikamaSceneUtility.CreateOrReplaceAsset(ref dirArr, dirPath);
            }
        }

        public static async Task BakeWithoutKanikamaAsync(BakingContext context, CancellationToken cancellationToken)
        {
            var config = context.BakingConfiguration;

            var handles = context.BakeableHandles;
            // var lightSourceGroupHandles = config.GetLightSourceGroups().Select(id => new ObjectHandle<LightSourceGroup>(id)).ToArray();

            foreach (var handle in handles)
            {
                handle.Initialize();
                handle.TurnOff();
            }

            // foreach (var lightSourceGroupHandle in lightSourceGroupHandles)
            // {
            //     foreach (var lightSource in lightSourceGroupHandle.Value.GetLightSources())
            //     {
            //         lightSource.Initialize();
            //         lightSource.TurnOff();
            //     }
            // }

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
                foreach (var handle in handles)
                {
                    handle.Clear();
                }

                // foreach (var lightSourceGroupHandle in lightSourceGroupHandles)
                // {
                //     foreach (var lightSource in lightSourceGroupHandle.Value.GetLightSources())
                //     {
                //         lightSource.Clear();
                //     }
                // }
            }
        }

        static string TempLightmapName(BakedLightmap bakedLightmap, string id)
        {
            var path = bakedLightmap.Path;
            var fileName = Path.GetFileName(path);
            var ext = Path.GetExtension(fileName);
            return $"{bakedLightmap.Type.ToFileName()}-{bakedLightmap.Index}-{id}{ext}";
        }
    }
}
