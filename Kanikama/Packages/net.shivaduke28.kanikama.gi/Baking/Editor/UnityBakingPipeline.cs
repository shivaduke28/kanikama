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
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Editor
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
            using (var copiedSceneHandle = KanikamaSceneUtility.CopySceneAsset(context.SceneAssetData))
            {
                try
                {
                    // open the copied scene
                    EditorSceneManager.OpenScene(copiedSceneHandle.SceneAssetData.Path);

                    var bakeableHandles = context.BakeTargetHandles;
                    var guid = AssetDatabase.AssetPathToGUID(copiedSceneHandle.SceneAssetData.Path);

                    // initialize all light source handles **after** the copied scene is opened
                    foreach (var handle in bakeableHandles)
                    {
                        handle.ReplaceSceneGuid(guid);
                        handle.Initialize();
                        handle.TurnOff();
                    }

                    // save scene
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

                    // turn off all light sources but kanikama ones
                    bool Filter(Object obj) => bakeableHandles.All(l => !l.Includes(obj));

                    var sceneGIContext = UnitySceneGIContext.GetGIContext(Filter);

                    sceneGIContext.TurnOff();

                    var dstDir = context.Setting.OutputAssetDirPath;
                    KanikamaSceneUtility.CreateFolderIfNecessary(dstDir);

                    var lightmapper = context.Lightmapper;

                    foreach (var handle in bakeableHandles)
                    {
                        Debug.LogFormat(KanikamaDebug.Format, $"baking... id: {handle.Id}.");
                        handle.TurnOn();
                        lightmapper.ClearCache();
                        await lightmapper.BakeAsync(cancellationToken);
                        handle.TurnOff();

                        var baked = KanikamaSceneUtility.GetLightmaps(copiedSceneHandle.SceneAssetData);
                        CopyBakedLightingAssetCollection(baked, out var copied, dstDir, handle.Id);

                        context.Setting.LightmapStorage.AddOrUpdate(handle.Id, copied);
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
                    case UnityLightmapType.Color:
                        {
                            var outPath = Path.Combine(dstDir, TempLightmapName(lightmap, id));
                            var copiedLightmap = KanikamaSceneUtility.CopyBakedLightmap(lightmap, outPath);
                            dst.Add(copiedLightmap);
                            Debug.LogFormat(KanikamaDebug.Format,
                                $"copying lightmap (index:{lightmap.Index}, type:{lightmap.Type}) {lightmap.Path} -> {outPath}");
                        }
                        break;
                    case UnityLightmapType.Directional:
                        {
                            var outPath = Path.Combine(dstDir, TempLightmapName(lightmap, id));
                            var copiedLightmap = KanikamaSceneUtility.CopyBakedLightmap(lightmap, outPath);
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

        public static void CreateAssets(UnityLightmapStorage unityLightmapStorage, string dstDirPath, TextureResizeType resizeType)
        {
            Debug.LogFormat(KanikamaDebug.Format, $"create assets (resize type: {resizeType})");
            KanikamaSceneUtility.CreateFolderIfNecessary(dstDirPath);

            var allLightmaps = unityLightmapStorage.Get();
            var lightmaps = allLightmaps.Where(lm => lm.Type == UnityLightmapType.Color).ToArray();
            var directionalMaps = allLightmaps.Where(lm => lm.Type == UnityLightmapType.Directional).ToArray();
            var maxIndex = lightmaps.Max(lightmap => lightmap.Index);
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
                    var lightPath = Path.Combine(dstDirPath, $"{UnityLightmapType.Color.ToFileName()}-{i}.asset");
                    KanikamaSceneUtility.CreateOrReplaceAsset(ref lightArr, lightPath);
                    Debug.LogFormat(KanikamaDebug.Format, $"create asset: {lightPath}");
                }
                if (dir.Count > 0)
                {
                    var dirArr = TextureUtility.CreateTexture2DArray(dir, isLinear: true, mipChain: true);
                    var dirPath = Path.Combine(dstDirPath, $"{UnityLightmapType.Directional.ToFileName()}-{i}.asset");
                    KanikamaSceneUtility.CreateOrReplaceAsset(ref dirArr, dirPath);
                    Debug.LogFormat(KanikamaDebug.Format, $"create asset: {dirPath}");
                }
            }
        }

        public static async Task BakeWithoutKanikamaAsync(BakingContext context, CancellationToken cancellationToken)
        {
            Debug.LogFormat(KanikamaDebug.Format, $"Unity pipeline without Kanikama start");
            var handles = context.BakeTargetHandles;

            foreach (var handle in handles)
            {
                handle.Initialize();
                handle.TurnOff();
            }

            try
            {
                var lightmapper = context.Lightmapper;
                lightmapper.ClearCache();
                await lightmapper.BakeAsync(cancellationToken);
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
                foreach (var handle in handles)
                {
                    handle.Clear();
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
