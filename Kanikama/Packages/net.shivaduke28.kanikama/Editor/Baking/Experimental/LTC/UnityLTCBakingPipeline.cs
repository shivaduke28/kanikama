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
            public UnityLightmapper Lightmapper { get; }
            public UnityBakingSetting Setting { get; }
            public IList<ILTCMonitorHandle> MonitorHandles { get; }

            public Context(SceneAssetData sceneAssetData, UnityLightmapper lightmapper, UnityBakingSetting setting, IList<ILTCMonitorHandle> monitorHandles)
            {
                SceneAssetData = sceneAssetData;
                Lightmapper = lightmapper;
                Setting = setting;
                MonitorHandles = monitorHandles;
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

                    // turn off all light sources but kanikama ones
                    var sceneGIContext = UnitySceneGIContext.GetGIContext(obj => context.MonitorHandles.All(h => !h.Includes(obj)));
                    sceneGIContext.TurnOff();


                    // NOTE: BakeTargets are supposed to use Unity Area Light.
                    // Set Bounce 1 when BakeTargets Renderers with emissive materials.;
                    context.Lightmapper.SetBounce(0);
                    foreach (var monitor in context.MonitorHandles)
                    {
                        Debug.LogFormat(KanikamaDebug.Format, $"baking LTC monitor w/ shadow... name: {monitor.Name}, id: {monitor.Id}.");
                        monitor.TurnOn();
                        monitor.SetCastShadow(true);
                        context.Lightmapper.ClearCache();
                        await context.Lightmapper.BakeAsync(cancellationToken);
                        var bakedShadows = UnityLightmapUtility.GetLightmaps(copiedScene.SceneAssetData).Where(l => l.Type == UnityLightmapType.Light).ToList();
                        UnityBakingPipeline.CopyBakedLightingAssetCollection(bakedShadows, out var copiedShadow, context.Setting.OutputAssetDirPath,
                            monitor.Id + "_shadow");
                        context.Setting.LightmapStorage.AddOrUpdate(monitor.Id + "_shadow", copiedShadow, monitor.Name + "_shadow");


                        Debug.LogFormat(KanikamaDebug.Format, $"baking LTC monitor w/o shadow... name: {monitor.Name}, id: {monitor.Id}.");
                        monitor.SetCastShadow(false);
                        context.Lightmapper.ClearCache();
                        await context.Lightmapper.BakeAsync(cancellationToken);
                        monitor.TurnOff();
                        var bakedNoShadows = UnityLightmapUtility.GetLightmaps(copiedScene.SceneAssetData).Where(l => l.Type == UnityLightmapType.Light)
                            .ToList();
                        UnityBakingPipeline.CopyBakedLightingAssetCollection(bakedNoShadows, out var copiedNoShadow, context.Setting.OutputAssetDirPath,
                            monitor.Id + "_ltc");
                        context.Setting.LightmapStorage.AddOrUpdate(monitor.Id + "_ltc", copiedNoShadow, monitor.Name + "_ltc");
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


        public static void CreateAssets(IList<ILTCMonitorHandle> monitors, UnityBakingSetting bakingSetting)
        {
            Assert.IsTrue(monitors.Count <= 3);
            Debug.LogFormat(KanikamaDebug.Format, $"create LTC assets (resize type: {bakingSetting.TextureResizeType})");

            var lightmapStorage = bakingSetting.LightmapStorage;
            var hasError = false;
            var maps = new Dictionary<int, List<(Texture2D Shadow, Texture2D Light)>>();
            foreach (var handle in monitors)
            {
                if (lightmapStorage.TryGet(handle.Id + "_ltc", out var lm) && lightmapStorage.TryGet(handle.Id + "_shadow", out var lms))
                {
                    foreach (var light in lm.Where(l => l.Type == UnityLightmapType.Light))
                    {
                        var shadow = lms.FirstOrDefault(s => s.Type == UnityLightmapType.Light && s.Index == light.Index);
                        if (shadow == null)
                        {
                            Debug.LogErrorFormat(KanikamaDebug.Format,
                                $"Shadow map not found in {nameof(UnityLightmapStorage)}. Name:{handle.Name}, Key:{handle.Id}");
                            hasError = true;
                            continue;
                        }

                        if (!maps.TryGetValue(light.Index, out var list))
                        {
                            list = new List<(Texture2D Shadow, Texture2D Light)>();
                            maps[light.Index] = list;
                        }
                        list.Add((shadow.Texture, light.Texture));
                    }
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

            var maxIndex = maps.Keys.Max();
            foreach (var (shadow, light) in maps.Values.SelectMany(l => l))
            {
                TextureUtility.ResizeTexture(shadow, bakingSetting.TextureResizeType);
                TextureUtility.ResizeTexture(light, bakingSetting.TextureResizeType);
            }

            for (var i = 0; i <= maxIndex; i++)
            {
                var packed = TextureUtility.RatioPackBC6H(maps[i].ToArray(), false);
                var path = Path.Combine(bakingSetting.OutputAssetDirPath, $"ltc-shadow-{i}.asset");
                IOUtility.CreateOrReplaceAsset(ref packed, path);
                Debug.LogFormat(KanikamaDebug.Format, $"create LTC asset: {path})");
            }
            Debug.LogFormat(KanikamaDebug.Format, "done");
        }
    }
}
