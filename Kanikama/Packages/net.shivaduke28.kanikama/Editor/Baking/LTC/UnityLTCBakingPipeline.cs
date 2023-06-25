using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Baking.Impl.LTC;
using Kanikama.Utility;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions;

namespace Kanikama.Editor.Baking.LTC
{
    public static class UnityLTCBakingPipeline
    {
        public const string LightmapKey = "KanikamaUnityLTC";
        public const string LightmapType = "LTCVisiblity";

        public readonly struct Parameter
        {
            public SceneAssetData SceneAssetData { get; }
            public UnityBakingSetting Setting { get; }
            public IList<LTCMonitor> Monitors { get; }

            public Parameter(SceneAssetData sceneAssetData, UnityBakingSetting setting, IList<LTCMonitor> monitors)
            {
                SceneAssetData = sceneAssetData;
                Setting = setting;
                Monitors = monitors;
            }
        }

        public static async Task BakeAsync(Parameter parameter, CancellationToken cancellationToken)
        {
            Assert.IsTrue(parameter.Monitors.Count <= 3);
            Debug.LogFormat(KanikamaDebug.Format, "Unity LTC Baking Pipeline Start");

            var commands = parameter.Monitors.Select(x => new UnityLTCBakingCommand(x)).Cast<IUnityBakingCommand>().ToArray();
            var param = new UnityBakingPipeline.Parameter(parameter.SceneAssetData, parameter.Setting, commands);
            try
            {
                await UnityBakingPipeline.BakeAsync(param, cancellationToken);
                Debug.LogFormat(KanikamaDebug.Format, "Unity LTC Baking Pipeline Done");
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
                EditorSceneManager.OpenScene(parameter.SceneAssetData.Path);
            }
        }


        public static void CreateAssets(IList<LTCMonitor> monitors, UnityBakingSetting bakingSetting)
        {
            Assert.IsTrue(monitors.Count <= 3);
            Debug.LogFormat(KanikamaDebug.Format, $"create LTC assets (resize type: {bakingSetting.TextureResizeType})");

            var lightmapStorage = bakingSetting.AssetStorage.LightmapStorage;
            var hasError = false;
            var maps = new Dictionary<int, List<(Texture2D Shadow, Texture2D Light)>>();
            var handles = monitors.Select(x => new UnityLTCBakingCommand(x));
            foreach (var handle in handles)
            {
                if (lightmapStorage.TryGet(handle.IdLTC, out var lm) && lightmapStorage.TryGet(handle.IdShadow, out var lms))
                {
                    foreach (var light in lm.Where(l => l.Type == UnityLightmap.Light))
                    {
                        var shadow = lms.FirstOrDefault(s => s.Type == UnityLightmap.Light && s.Index == light.Index);
                        if (shadow == null)
                        {
                            Debug.LogErrorFormat(KanikamaDebug.Format,
                                $"Shadow map not found in {nameof(LightmapStorage)}. Name:{handle.Name}, Key:{handle.Id}");
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
                    Debug.LogErrorFormat(KanikamaDebug.Format, $"Lightmaps not found in {nameof(LightmapStorage)}. Name:{handle.Name}, Key:{handle.Id}");
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

            var output = new List<Lightmap>();
            for (var i = 0; i <= maxIndex; i++)
            {
                var packed = TextureUtility.RatioPackBC6H(maps[i].ToArray(), false);
                var path = Path.Combine(bakingSetting.OutputAssetDirPath, $"ltc-visibility-{i}.asset");
                IOUtility.CreateOrReplaceAsset(ref packed, path);
                output.Add(new Lightmap(LightmapType, packed, path, i));
                Debug.LogFormat(KanikamaDebug.Format, $"create LTC asset: {path})");
            }

            bakingSetting.AssetStorage.LightmapStorage.AddOrUpdate(LightmapKey, output, LightmapKey);
            Debug.LogFormat(KanikamaDebug.Format, "done");
        }
    }
}
