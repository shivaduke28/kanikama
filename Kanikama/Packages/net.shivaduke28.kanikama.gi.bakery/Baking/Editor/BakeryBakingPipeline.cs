using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core;
using Kanikama.Core.Editor;
using Kanikama.Core.Editor.Textures;
using Kanikama.GI.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Bakery.Editor
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
            using (var copiedSceneHandle = CopiedSceneAsset.Create(context.SceneAssetData, true))
            {
                try
                {
                    // open the copied scene
                    EditorSceneManager.OpenScene(copiedSceneHandle.SceneAssetData.Path);

                    var bakeTargetHandles = context.BakeTargetHandles;
                    var guid = AssetDatabase.AssetPathToGUID(copiedSceneHandle.SceneAssetData.Path);

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
                    KanikamaSceneUtility.CreateFolderIfNecessary(dstDir);

                    var map = new Dictionary<string, List<BakeryLightmap>>();
                    var lightmapper = context.Lightmapper;
                    var outputAssetDirPath = copiedSceneHandle.SceneAssetData.LightingAssetDirectoryPath;
                    // NOTE: need to set output path explicitly to Bakery.
                    lightmapper.SetOutputAssetDirPath(outputAssetDirPath);

                    // TODO: Lightmapperのパラメータ指定があるはず
                    foreach (var handle in bakeTargetHandles)
                    {
                        Debug.LogFormat(KanikamaDebug.Format, $"baking... id: {handle.Id}.");
                        handle.TurnOn();
                        await lightmapper.BakeAsync(cancellationToken);
                        handle.TurnOff();

                        var baked = KanikamaBakeryUtility.GetLightmaps(outputAssetDirPath, copiedSceneHandle.SceneAssetData.Asset.name);
                        Copy(baked, out var copied, dstDir, handle.Id);
                        map[handle.Id] = copied;
                    }

                    foreach (var kvp in map)
                    {
                        context.Setting.LightmapStorage.AddOrUpdate(kvp.Key, kvp.Value);
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

        public static async Task BakeWithoutKanikamaAsync(Context context, CancellationToken cancellationToken)
        {
            Debug.LogFormat(KanikamaDebug.Format, "Bakery pipeline without Kanikama start");
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

        public static void CreateAssets(BakeryBakingSetting setting)
        {
            Debug.LogFormat(KanikamaDebug.Format, $"create assets (resize type: {setting.TextureResizeType})");
            var dstDirPath = setting.OutputAssetDirPath;
            var resizeType = setting.TextureResizeType;
            KanikamaSceneUtility.CreateFolderIfNecessary(dstDirPath);

            var allLightmaps = setting.LightmapStorage.Get();
            var lightmaps = allLightmaps.Where(lm => lm.Type == BakeryLightmapType.Color).ToArray();
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
                    var lightPath = Path.Combine(dstDirPath, $"{UnityLightmapType.Color.ToFileName()}-{index}.asset");
                    if (lightArr != null)
                    {
                        KanikamaSceneUtility.CreateOrReplaceAsset(ref lightArr, lightPath);
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
                    var dirPath = Path.Combine(dstDirPath, $"{UnityLightmapType.Directional.ToFileName()}-{index}.asset");
                    if (dirArr != null)
                    {
                        KanikamaSceneUtility.CreateOrReplaceAsset(ref dirArr, dirPath);
                        Debug.LogFormat(KanikamaDebug.Format, $"create asset: {dirPath}");
                    }
                }
            }
        }
    }
}
