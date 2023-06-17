using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Utility;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kanikama.Editor.Baking
{
    public static class UnityBakingPipeline
    {
        public sealed class Parameter
        {
            public SceneAssetData SceneAssetData { get; }
            public UnityLightmapper Lightmapper { get; }
            public UnityBakingSetting Setting { get; }
            public List<IUnityBakingCommand> Commands { get; }

            public Parameter(SceneAssetData sceneAssetData, UnityLightmapper lightmapper,
                UnityBakingSetting setting, List<IUnityBakingCommand> commands)
            {
                SceneAssetData = sceneAssetData;
                Lightmapper = lightmapper;
                Setting = setting;
                Commands = commands;
            }
        }

        public sealed class Context
        {
            public SceneAssetData SceneAssetData { get; } // copied
            public UnityLightmapper Lightmapper { get; }
            public UnityBakingSetting Setting { get; }

            public Context(SceneAssetData sceneAssetData, UnityLightmapper lightmapper,
                UnityBakingSetting setting)
            {
                SceneAssetData = sceneAssetData;
                Lightmapper = lightmapper;
                Setting = setting;
            }
        }

        public static async Task BakeAsync(Parameter parameter, CancellationToken cancellationToken)
        {
            Debug.LogFormat(KanikamaDebug.Format, "Unity pipeline start");

            IOUtility.CreateFolderIfNecessary(parameter.Setting.OutputAssetDirPath);
            using (var copiedScene = CopiedSceneAsset.Create(parameter.SceneAssetData, true))
            {
                var context = new Context(copiedScene.SceneAssetData, parameter.Lightmapper, parameter.Setting);
                try
                {
                    // open the copied scene
                    EditorSceneManager.OpenScene(copiedScene.SceneAssetData.Path);
                    // Turn off all light sources
                    UnitySceneGIContext.GetGIContext().TurnOff();

                    // initialize all light source handles **after** the copied scene is opened
                    foreach (var command in parameter.Commands)
                    {
                        command.Initialize(copiedScene.SceneAssetData.Guid);
                    }

                    // Run all baking command
                    foreach (var command in parameter.Commands)
                    {
                        await command.RunAsync(context, cancellationToken);
                    }

                    var settingAsset = UnityBakingSettingAsset.FindOrCreate(parameter.SceneAssetData.Asset);
                    settingAsset.Setting = parameter.Setting;
                    EditorUtility.SetDirty(settingAsset);
                    AssetDatabase.SaveAssets();
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
                    EditorSceneManager.OpenScene(parameter.SceneAssetData.Path);
                }
            }
        }

        public static List<UnityLightmap> CopyBakedLightingAssetCollection(List<UnityLightmap> src, string dstDir, string id)
        {
            var dst = new List<UnityLightmap>(src.Count);
            foreach (var lightmap in src)
            {
                switch (lightmap.Type)
                {
                    case UnityLightmapType.Light:
                    case UnityLightmapType.Directional:
                        var outPath = Path.Combine(dstDir, TempLightmapName(lightmap, id));
                        dst.Add(UnityLightmapUtility.CopyBakedLightmap(lightmap, outPath));
                        break;
                    case UnityLightmapType.ShadowMask:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return dst;
        }

        public static void CreateAssets(IEnumerable<IBakeTargetHandle> bakeTargetHandles, UnityBakingSetting bakingSetting)
        {
            var resizeType = bakingSetting.TextureResizeType;
            var dstDirPath = bakingSetting.OutputAssetDirPath;
            var lightmapStorage = bakingSetting.LightmapStorage;
            Debug.LogFormat(KanikamaDebug.Format, $"create assets (resize type: {resizeType})");
            IOUtility.CreateFolderIfNecessary(dstDirPath);

            // check lightmaps are stored in storage
            var allLightmaps = new List<UnityLightmap>();
            var hasError = false;
            foreach (var handle in bakeTargetHandles)
            {
                if (lightmapStorage.TryGet(handle.Id, out var lm))
                {
                    allLightmaps.AddRange(lm);
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

            var lightmaps = allLightmaps.Where(lm => lm.Type == UnityLightmapType.Light).ToArray();
            var directionalMaps = allLightmaps.Where(lm => lm.Type == UnityLightmapType.Directional).ToArray();
            var maxIndex = lightmaps.Max(lightmap => lightmap.Index);
            foreach (var lm in allLightmaps)
            {
                TextureUtility.ResizeTexture(lm.Texture, resizeType);
            }

            bakingSetting.LightmapArrayStorage.Clear();
            for (var i = 0; i <= maxIndex; i++)
            {
                var index = i;
                var light = lightmaps.Where(l => l.Index == index).Select(l => l.Texture).ToList();
                var dir = directionalMaps.Where(l => l.Index == index).Select(l => l.Texture).ToList();

                if (light.Count > 0)
                {
                    var lightArr = TextureUtility.CreateTexture2DArray(light, isLinear: false, mipChain: true);
                    var lightPath = Path.Combine(dstDirPath, $"{UnityLightmapType.Light.ToFileName()}-{i}.asset");
                    IOUtility.CreateOrReplaceAsset(ref lightArr, lightPath);
                    bakingSetting.LightmapArrayStorage.AddOrUpdate(new UnityLightmapArray(UnityLightmapType.Light, lightArr, lightPath, i));
                    Debug.LogFormat(KanikamaDebug.Format, $"create asset: {lightPath}");
                }
                if (dir.Count > 0)
                {
                    var dirArr = TextureUtility.CreateTexture2DArray(dir, isLinear: true, mipChain: true);
                    var dirPath = Path.Combine(dstDirPath, $"{UnityLightmapType.Directional.ToFileName()}-{i}.asset");
                    IOUtility.CreateOrReplaceAsset(ref dirArr, dirPath);
                    bakingSetting.LightmapArrayStorage.AddOrUpdate(new UnityLightmapArray(UnityLightmapType.Directional, dirArr, dirPath, i));
                    Debug.LogFormat(KanikamaDebug.Format, $"create asset: {dirPath}");
                }
            }
            Debug.LogFormat(KanikamaDebug.Format, "done");
        }

        public static async Task BakeStaticAsync(Parameter parameter, CancellationToken cancellationToken)
        {
            Debug.LogFormat(KanikamaDebug.Format, "Unity pipeline (static) start");
            using (var copiedScene = CopiedSceneAsset.Create(parameter.SceneAssetData, false, "_kanikama_static_unity"))
            {
                // open the copied scene
                EditorSceneManager.OpenScene(copiedScene.SceneAssetData.Path);

                // initialize all light source handles **after** the copied scene is opened
                foreach (var command in parameter.Commands)
                {
                    command.Initialize(copiedScene.SceneAssetData.Guid);
                }


                try
                {
                    var lightmapper = parameter.Lightmapper;
                    lightmapper.ClearCache();
                    await lightmapper.BakeAsync(cancellationToken);
                    var lightingDataAsset = Lightmapping.lightingDataAsset;
                    await LightingDataAssetReplacer.ReplaceAsync(parameter.SceneAssetData.Asset, lightingDataAsset, cancellationToken);

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
                    EditorSceneManager.OpenScene(parameter.SceneAssetData.Path);
                }
            }
        }

        static string TempLightmapName(UnityLightmap unityLightmap, string id)
        {
            var path = unityLightmap.Path;
            var fileName = Path.GetFileName(path);
            var ext = Path.GetExtension(fileName);
            return $"{unityLightmap.Type.ToFileName()}-{unityLightmap.Index}-{id}{ext}";
        }
    }
}
