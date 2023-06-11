using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Utility;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions;

namespace Kanikama.Editor.Baking.Experimental.LTC
{
    public static class UnityLTCBakingPipeline
    {
        public sealed class Context
        {
            public SceneAssetData SceneAssetData { get; }
            public IList<IBakeTargetHandle> GridFiberHandles { get; }
            public IList<IBakeTargetHandle> MonitorHandles { get; }
            public UnityLightmapper Lightmapper { get; }
            public UnityBakingSetting Setting { get; }

            public Context(SceneAssetData sceneAssetData,
                IList<IBakeTargetHandle> gridFiberHandles,
                IList<IBakeTargetHandle> monitorHandles,
                UnityLightmapper lightmapper,
                UnityBakingSetting setting)
            {
                SceneAssetData = sceneAssetData;
                GridFiberHandles = gridFiberHandles;
                MonitorHandles = monitorHandles;
                Lightmapper = lightmapper;
                Setting = setting;
            }
        }

        public static async Task BakeAsync(Context context, CancellationToken cancellationToken)
        {
            Assert.IsTrue(context.MonitorHandles.Count <= 3);
            Debug.LogFormat(KanikamaDebug.Format, "Unity LTC Baking Pipeline Start");
            using (var copiedScene = CopiedSceneAsset.Create(context.SceneAssetData, true))
            {
                try
                {
                    // open the copied scene
                    EditorSceneManager.OpenScene(copiedScene.SceneAssetData.Path);

                    // initialize all light source handles **after** the copied scene is opened
                    var copiedSceneGuid = copiedScene.SceneAssetData.Guid;
                    foreach (var monitor in context.MonitorHandles)
                    {
                        monitor.Initialize(copiedSceneGuid);
                    }

                    foreach (var handle in context.GridFiberHandles)
                    {
                        handle.Initialize(copiedSceneGuid);
                    }

                    // turn off all light sources but kanikama ones
                    var sceneGIContext = UnitySceneGIContext.GetGIContext(obj => context.GridFiberHandles.All(h => !h.Includes(obj)));
                    sceneGIContext.TurnOff();


                    // bake lightmaps for each grid fibers
                    foreach (var handle in context.GridFiberHandles)
                    {
                        Debug.LogFormat(KanikamaDebug.Format, $"baking... name: {handle.Name}, id: {handle.Id}.");
                        handle.TurnOn();
                        context.Lightmapper.ClearCache();
                        await context.Lightmapper.BakeAsync(cancellationToken);
                        handle.TurnOff();
                        var baked = UnityLightmapUtility.GetLightmaps(copiedScene.SceneAssetData);
                        UnityBakingPipeline.CopyBakedLightingAssetCollection(baked, out var copied, context.Setting.OutputAssetDirPath, handle.Id);
                        context.Setting.LightmapStorage.AddOrUpdate(handle.Id, copied, handle.Name);
                    }

                    // NOTE: BakeTargets are supposed to use Unity Area Light.
                    // Set Bounce 1 when BakeTargets Renderers with emissive materials.;
                    context.Lightmapper.SetBounce(0);
                    foreach (var monitor in context.MonitorHandles)
                    {
                        Debug.LogFormat(KanikamaDebug.Format, $"baking LTC monitor... name: {monitor.Name}, id: {monitor.Id}.");
                        monitor.TurnOn();
                        context.Lightmapper.ClearCache();
                        await context.Lightmapper.BakeAsync(cancellationToken);
                        monitor.TurnOff();
                        var bakedShadows = UnityLightmapUtility.GetLightmaps(copiedScene.SceneAssetData);
                        UnityBakingPipeline.CopyBakedLightingAssetCollection(bakedShadows, out var copied, context.Setting.OutputAssetDirPath, monitor.Id);
                        context.Setting.LightmapStorage.AddOrUpdate(monitor.Id, copied, monitor.Name);
                    }
                    Debug.LogFormat(KanikamaDebug.Format, "done");
                }
                catch (OperationCanceledException)
                {
                    Debug.LogFormat(KanikamaDebug.Format, "canceled");
                    throw;
                }
                catch (Exception e)
                {
                    Debug.LogFormat(KanikamaDebug.Format, "failed");
                    Debug.LogException(e);
                }
                finally
                {
                    EditorSceneManager.OpenScene(context.SceneAssetData.Path);
                }
            }
        }


        public static void CreateAssets(IList<IBakeTargetHandle> gridFiberHandles, IList<IBakeTargetHandle> monitorHandles, UnityBakingSetting bakingSetting)
        {
            Assert.IsTrue(monitorHandles.Count <= 3);

            // create PRT assets
            UnityBakingPipeline.CreateAssets(gridFiberHandles, bakingSetting);
            var lightmapStorage = bakingSetting.LightmapStorage;
            var hasError = false;
            var lightmaps = new List<UnityLightmap>();
            foreach (var handle in monitorHandles)
            {
                if (lightmapStorage.TryGet(handle.Id, out var lm))
                {
                    lightmaps.AddRange(lm.Where(l => l.Type == UnityLightmapType.Light));
                }
                else
                {
                    Debug.LogErrorFormat(KanikamaDebug.Format, $"Lightmaps not found in {nameof(UnityLightmapStorage)}. Name:{handle.Name}, Key:{handle.Id}");
                    hasError = true;
                }
            }
            if (hasError)
            {
                Debug.LogErrorFormat(KanikamaDebug.Format, "canceled by some error.");
                return;
            }
            var maxIndex = lightmaps.Max(lightmap => lightmap.Index);
            foreach (var lm in lightmaps)
            {
                TextureUtility.ResizeTexture(lm.Texture, bakingSetting.TextureResizeType);
            }
            for (var i = 0; i <= maxIndex; i++)
            {
                var light = lightmaps.Where(l => l.Index == i).Select(l => l.Texture).ToArray();
                var packed = TextureUtility.PackBC6H(light, false);
                var path = Path.Combine(bakingSetting.OutputAssetDirPath, $"shadow-{i}.asset");
                IOUtility.CreateOrReplaceAsset(ref packed, path);
            }
        }
    }
}
