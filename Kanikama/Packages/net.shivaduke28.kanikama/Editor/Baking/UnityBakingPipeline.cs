﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Utility;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Kanikama.Editor.Baking
{
    public static class UnityBakingPipeline
    {
        public sealed class BakingContext
        {
            public SceneAssetData SceneAssetData { get; }
            public List<IBakeTargetHandle> BakeTargetHandles { get; }
            public UnityLightmapper Lightmapper { get; }
            public UnityBakingSetting Setting { get; }

            public BakingContext(SceneAssetData sceneAssetData, List<IBakeTargetHandle> bakeTargetHandles, UnityLightmapper lightmapper,
                UnityBakingSetting setting)
            {
                SceneAssetData = sceneAssetData;
                BakeTargetHandles = bakeTargetHandles;
                Lightmapper = lightmapper;
                Setting = setting;
            }
        }

        public static async Task BakeAsync(BakingContext context, CancellationToken cancellationToken)
        {
            Debug.LogFormat(KanikamaDebug.Format, "Unity pipeline start");
            using (var copiedScene = CopiedSceneAsset.Create(context.SceneAssetData, true))
            {
                try
                {
                    // open the copied scene
                    EditorSceneManager.OpenScene(copiedScene.SceneAssetData.Path);

                    var bakeTargetHandles = context.BakeTargetHandles;
                    var copiedSceneGuid = copiedScene.SceneAssetData.Guid;

                    // initialize all light source handles **after** the copied scene is opened
                    foreach (var handle in bakeTargetHandles)
                    {
                        handle.Initialize(copiedSceneGuid);
                        handle.TurnOff();
                    }

                    // save scene
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

                    // turn off all light sources but kanikama ones
                    bool Filter(Object obj) => bakeTargetHandles.All(l => !l.Includes(obj));

                    var sceneGIContext = UnitySceneGIContext.GetGIContext(Filter);

                    sceneGIContext.TurnOff();

                    var dstDir = context.Setting.OutputAssetDirPath;
                    IOUtility.CreateFolderIfNecessary(dstDir);

                    var lightmapper = context.Lightmapper;

                    foreach (var handle in bakeTargetHandles)
                    {
                        Debug.LogFormat(KanikamaDebug.Format, $"baking... name: {handle.Name}, id: {handle.Id}.");
                        handle.TurnOn();
                        lightmapper.ClearCache();
                        await lightmapper.BakeAsync(cancellationToken);
                        handle.TurnOff();

                        var baked = UnityLightmapUtility.GetLightmaps(copiedScene.SceneAssetData);
                        CopyBakedLightingAssetCollection(baked, out var copied, dstDir, handle.Id);

                        context.Setting.LightmapStorage.AddOrUpdate(handle.Id, copied, handle.Name);
                    }

                    var settingAsset = UnityBakingSettingAsset.FindOrCreate(context.SceneAssetData.Asset);
                    settingAsset.Setting = context.Setting;
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
                    EditorSceneManager.OpenScene(context.SceneAssetData.Path);
                }
            }
        }

        static void CopyBakedLightingAssetCollection(List<UnityLightmap> src, out List<UnityLightmap> dst, string dstDir,
            string id)
        {
            dst = new List<UnityLightmap>(src.Count);
            foreach (var lightmap in src)
            {
                switch (lightmap.Type)
                {
                    case UnityLightmapType.Light:
                        {
                            var outPath = Path.Combine(dstDir, TempLightmapName(lightmap, id));
                            var copiedLightmap = UnityLightmapUtility.CopyBakedLightmap(lightmap, outPath);
                            dst.Add(copiedLightmap);
                            Debug.LogFormat(KanikamaDebug.Format,
                                $"copying lightmap (index:{lightmap.Index}, type:{lightmap.Type}) {lightmap.Path} -> {outPath}");
                        }
                        break;
                    case UnityLightmapType.Directional:
                        {
                            var outPath = Path.Combine(dstDir, TempLightmapName(lightmap, id));
                            var copiedLightmap = UnityLightmapUtility.CopyBakedLightmap(lightmap, outPath);
                            dst.Add(copiedLightmap);
                            Debug.LogFormat(KanikamaDebug.Format,
                                $"copying lightmap (index:{lightmap.Index}, type:{lightmap.Type}) {lightmap.Path} -> {outPath}");
                        }
                        break;
                    case UnityLightmapType.ShadowMask:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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

            bakingSetting.LightmapArrayStorage.Clear();
            for (var i = 0; i <= maxIndex; i++)
            {
                var index = i;
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

        public static async Task BakeStaticAsync(BakingContext context, CancellationToken cancellationToken)
        {
            Debug.LogFormat(KanikamaDebug.Format, "Unity pipeline (static) start");
            using (var copiedScene = CopiedSceneAsset.Create(context.SceneAssetData, false, "_kanikama_static_unity"))
            {
                // open the copied scene
                EditorSceneManager.OpenScene(copiedScene.SceneAssetData.Path);

                var bakeTargetHandles = context.BakeTargetHandles;
                var copiedSceneGuid = copiedScene.SceneAssetData.Guid;

                // initialize all light source handles **after** the copied scene is opened
                foreach (var handle in bakeTargetHandles)
                {
                    handle.Initialize(copiedSceneGuid);
                    handle.TurnOff();
                }

                try
                {
                    var lightmapper = context.Lightmapper;
                    lightmapper.ClearCache();
                    await lightmapper.BakeAsync(cancellationToken);
                    var lightingDataAsset = Lightmapping.lightingDataAsset;
                    await LightingDataAssetReplacer.ReplaceAsync(context.SceneAssetData.Asset, lightingDataAsset, cancellationToken);

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

        static string TempLightmapName(UnityLightmap unityLightmap, string id)
        {
            var path = unityLightmap.Path;
            var fileName = Path.GetFileName(path);
            var ext = Path.GetExtension(fileName);
            return $"{unityLightmap.Type.ToFileName()}-{unityLightmap.Index}-{id}{ext}";
        }
    }
}