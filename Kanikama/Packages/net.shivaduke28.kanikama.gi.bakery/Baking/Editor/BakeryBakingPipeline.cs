using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core;
using Kanikama.Core.Editor;
using Kanikama.Core.Editor.Util;
using Kanikama.GI.Baking.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Bakery.Baking.Editor
{
    public static class BakeryBakingPipeline
    {
        public sealed class Context
        {
            public SceneAssetData SceneAssetData { get; }
            public List<IBakeTargetHandle> BakeTargetHandles { get; }
            public BakeryLightmapper Lightmapper { get; }
            public BakeryBakingSetting Setting { get; }

            public Context(SceneAssetData sceneAssetData,
                List<IBakeTargetHandle> bakeTargetHandles,
                BakeryLightmapper lightmapper,
                BakeryBakingSetting setting)
            {
                SceneAssetData = sceneAssetData;
                BakeTargetHandles = bakeTargetHandles;
                Lightmapper = lightmapper;
                Setting = setting;
            }
        }

        public static async Task BakeAsync(Context context, CancellationToken cancellationToken)
        {
            Debug.LogFormat(KanikamaDebug.Format, "Bakery pipeline start");
            using (var copiedScene = CopiedSceneAsset.Create(context.SceneAssetData, true))
            {
                try
                {
                    // open the copied scene
                    EditorSceneManager.OpenScene(copiedScene.SceneAssetData.Path);

                    var bakeTargetHandles = context.BakeTargetHandles;
                    var guid = copiedScene.SceneAssetData.Guid;

                    // initialize all light source handles **after** the copied scene is opened
                    foreach (var handle in bakeTargetHandles)
                    {
                        handle.Initialize(guid);
                        handle.TurnOff();
                    }

                    // save scene
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

                    // turn off all light sources but kanikama ones
                    bool Filter(Object obj) => bakeTargetHandles.All(l => !l.Includes(obj));

                    var sceneGIContext = BakerySceneGIContext.GetContext(Filter);
                    sceneGIContext.TurnOff();

                    var dstDir = context.Setting.OutputAssetDirPath;
                    IOUtility.CreateFolderIfNecessary(dstDir);

                    var lightmapper = context.Lightmapper;
                    var outputAssetDirPath = copiedScene.SceneAssetData.LightingAssetDirectoryPath;
                    // NOTE: need to set output path explicitly to Bakery.
                    lightmapper.SetOutputAssetDirPath(outputAssetDirPath);

                    // TODO: Lightmapperのパラメータ指定があるはず
                    foreach (var handle in bakeTargetHandles)
                    {
                        Debug.LogFormat(KanikamaDebug.Format, $"baking... name:{handle.Name}, id: {handle.Id}.");
                        handle.TurnOn();
                        await lightmapper.BakeAsync(cancellationToken);
                        handle.TurnOff();

                        var baked = KanikamaBakeryUtility.GetLightmaps(outputAssetDirPath, copiedScene.SceneAssetData.Asset.name);
                        Copy(baked, out var copied, dstDir, handle.Id);
                        context.Setting.LightmapStorage.AddOrUpdate(handle.Id, copied, handle.Name);
                    }

                    // NOTE: A SettingAsset object may be destroyed while awaiting baking for some reason...
                    // So we use Setting class and update asset after baking done.
                    var settingAssets = BakeryBakingSettingAsset.FindOrCreate(context.SceneAssetData.Asset);
                    settingAssets.Setting = context.Setting;
                    EditorUtility.SetDirty(settingAssets);
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

        static void Copy(List<BakeryLightmap> src, out List<BakeryLightmap> dst, string dstDir, string id)
        {
            dst = new List<BakeryLightmap>(src.Count);
            foreach (var lightmap in src)
            {
                var dstPath = Path.Combine(dstDir, CopiedLightmapName(lightmap, id));
                AssetDatabase.CopyAsset(lightmap.Path, dstPath);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(dstPath);
                var copied = new BakeryLightmap(lightmap.Type, texture, dstPath, lightmap.Index);
                dst.Add(copied);
                Debug.LogFormat(KanikamaDebug.Format, $"copying lightmap (index:{lightmap.Index}, type:{lightmap.Type}) {lightmap.Path} -> {dstPath}");
            }
        }

        public static async Task BakeStaticAsync(Context context, CancellationToken cancellationToken)
        {
            Debug.LogFormat(KanikamaDebug.Format, "Bakery pipeline non Kanikama start");
            var handles = context.BakeTargetHandles;
            var guid = AssetDatabase.AssetPathToGUID(context.SceneAssetData.Path);

            foreach (var handle in handles)
            {
                handle.Initialize(guid);
                handle.TurnOff();
            }

            try
            {
                var lightmapper = context.Lightmapper;
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

        static string CopiedLightmapName(BakeryLightmap bakedLightmap, string id)
        {
            var path = bakedLightmap.Path;
            var fileName = Path.GetFileName(path);
            var ext = Path.GetExtension(fileName);
            return $"{bakedLightmap.Type.ToString()}-{bakedLightmap.Index}-{id}{ext}";
        }

        public static void CreateAssets(IEnumerable<IBakeTargetHandle> bakeTargetHandles, BakeryBakingSetting setting)
        {
            Debug.LogFormat(KanikamaDebug.Format, $"create assets (resize type: {setting.TextureResizeType})");
            var dstDirPath = setting.OutputAssetDirPath;
            var resizeType = setting.TextureResizeType;
            var lightmapStorage = setting.LightmapStorage;
            IOUtility.CreateFolderIfNecessary(dstDirPath);

            // check lightmaps are stored in storage
            var allLightmaps = new List<BakeryLightmap>();
            var hasError = false;
            foreach (var handle in bakeTargetHandles)
            {
                if (lightmapStorage.TryGet(handle.Id, out var lm))
                {
                    allLightmaps.AddRange(lm);
                }
                else
                {
                    Debug.LogErrorFormat(KanikamaDebug.Format, $"Lightmaps not found in {nameof(BakeryLightmapStorage)}. Name:{handle.Name}, Key:{handle.Id}");
                    hasError = true;
                }
            }
            if (hasError)
            {
                Debug.LogErrorFormat(KanikamaDebug.Format, "canceled by some error.");
                return;
            }

            setting.LightmapArrayStorage.Clear();
            var lightmaps = allLightmaps.Where(lm => lm.Type == BakeryLightmapType.Light).ToArray();
            var directionalMaps = allLightmaps.Where(lm => lm.Type == BakeryLightmapType.Directional).ToArray();
            var maxIndex = lightmaps.Max(lightmap => lightmap.Index);
            for (var index = 0; index <= maxIndex; index++)
            {
                var light = lightmaps.Where(l => l.Index == index).Select(l => l.Texture).ToList();
                var dir = directionalMaps.Where(l => l.Index == index).Select(l => l.Texture).ToList();

                if (light.Count > 0)
                {
                    foreach (var texture in light)
                    {
                        TextureUtility.ResizeTexture(texture, resizeType);
                    }
                    // FIXME: validate all or no lightmaps have mipmap.
                    var useMipmap = TextureUtility.GetTextureHasMipmap(light[0]);
                    var lightArr = TextureUtility.CreateTexture2DArray(light, isLinear: false, mipChain: useMipmap);
                    var lightPath = Path.Combine(dstDirPath, $"{BakeryLightmapType.Light.ToFileName()}-{index}.asset");
                    if (lightArr != null)
                    {
                        IOUtility.CreateOrReplaceAsset(ref lightArr, lightPath);
                        setting.LightmapArrayStorage.AddOrUpdate(new BakeryLightmapArray(BakeryLightmapType.Light, lightArr, lightPath, index));
                        Debug.LogFormat(KanikamaDebug.Format, $"create asset: {lightPath}");
                    }
                }
                if (dir.Count > 0)
                {
                    foreach (var texture in dir)
                    {
                        TextureUtility.ResizeTexture(texture, resizeType);
                    }
                    // FIXME: check all or no lightmaps have mipmap.
                    var useMipmap = TextureUtility.GetTextureHasMipmap(dir[0]);
                    var dirArr = TextureUtility.CreateTexture2DArray(dir, isLinear: true, mipChain: useMipmap);
                    var dirPath = Path.Combine(dstDirPath, $"{BakeryLightmapType.Directional.ToFileName()}-{index}.asset");
                    if (dirArr != null)
                    {
                        IOUtility.CreateOrReplaceAsset(ref dirArr, dirPath);
                        setting.LightmapArrayStorage.AddOrUpdate(new BakeryLightmapArray(BakeryLightmapType.Directional, dirArr, dirPath, index));
                        Debug.LogFormat(KanikamaDebug.Format, $"create asset: {dirPath}");
                    }
                }
            }
            Debug.LogFormat(KanikamaDebug.Format, "done");
        }
    }
}
