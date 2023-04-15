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

            public Context(SceneAssetData sceneAssetData,
                List<IBakeTargetHandle> bakeTargetHandles,
                BakeryLightmapper lightmapper)
            {
                SceneAssetData = sceneAssetData;
                BakeTargetHandles = bakeTargetHandles;
                Lightmapper = lightmapper;
            }
        }

        public static async Task BakeAsync(Context context, CancellationToken cancellationToken)
        {
            using (var copiedSceneHandle = KanikamaSceneUtility.CopySceneAsset(context.SceneAssetData))
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
                        handle.ReplaceSceneGuid(guid);
                        handle.Initialize();
                        handle.TurnOff();
                    }

                    // save scene
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

                    // turn off all light sources but kanikama ones
                    bool Filter(Object obj) => bakeTargetHandles.All(l => !l.Includes(obj));

                    var sceneGIContext = BakerySceneGIContext.GetContext(Filter);
                    sceneGIContext.TurnOff();

                    // TODO: setting assetをパスを共通化...
                    var dstDir = $"{context.SceneAssetData.LightingAssetDirectoryPath}_kanikama_bakery";
                    KanikamaSceneUtility.CreateFolderIfNecessary(dstDir);

                    var map = new Dictionary<string, List<BakeryLightmap>>();
                    var lightmapper = context.Lightmapper;
                    var outputDirPath = lightmapper.OutputAssetDirPath;

                    // TODO: Lightmapperのパラメータ指定があるはず
                    foreach (var handle in bakeTargetHandles)
                    {
                        handle.TurnOn();
                        await lightmapper.BakeAsync(cancellationToken);
                        handle.TurnOff();

                        var baked = KanikamaBakeryUtility.GetLightmaps(outputDirPath, copiedSceneHandle.SceneAssetData.Asset.name);
                        Copy(baked, out var copied, dstDir, handle.Id);
                        map[handle.Id] = copied;
                    }
                    var assets = BakeryBakingSettingAsset.FindOrCreate(context.SceneAssetData.Asset);
                    foreach (var kvp in map)
                    {
                        assets.AddOrUpdate(kvp.Key, kvp.Value);
                    }
                    EditorUtility.SetDirty(assets);
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
            }
        }

        static string CopiedLightmapName(BakeryLightmap bakedLightmap, string id)
        {
            var path = bakedLightmap.Path;
            var fileName = Path.GetFileName(path);
            var ext = Path.GetExtension(fileName);
            return $"{bakedLightmap.Type.ToString()}-{bakedLightmap.Index}-{id}{ext}";
        }

        public static void CreateAssets(BakeryBakingSettingAsset settingAsset, string dstDirPath, TextureResizeType resizeType)
        {
            KanikamaSceneUtility.CreateFolderIfNecessary(dstDirPath);

            var allLightmaps = settingAsset.GetBakeryLightmaps();
            var lightmaps = allLightmaps.Where(lm => lm.Type == BakeryLightmapType.Color).ToArray();
            var directionalMaps = allLightmaps.Where(lm => lm.Type == BakeryLightmapType.Directional).ToArray();
            var maxIndex = lightmaps.Max(lightmap => lightmap.Index);
            for (var index = 0; index <= maxIndex; index++)
            {
                var light = lightmaps.Where(l => l.Index == index).Select(l => l.Texture).ToList();
                var dir = directionalMaps.Where(l => l.Index == index).Select(l => l.Texture).ToList();

                // TODO: mipChainは設定を見る方がいいかもしれん
                if (light.Count > 0)
                {
                    foreach (var texture in light)
                    {
                        TextureUtility.ResizeTexture(texture, resizeType);
                    }
                    var lightArr = TextureUtility.CreateTexture2DArray(light, isLinear: false, mipChain: false);
                    var lightPath = Path.Combine(dstDirPath, $"{LightmapType.Color.ToFileName()}-{index}.asset");
                    if (lightArr != null)
                    {
                        KanikamaSceneUtility.CreateOrReplaceAsset(ref lightArr, lightPath);
                    }
                }
                if (dir.Count > 0)
                {
                    foreach (var texture in dir)
                    {
                        TextureUtility.ResizeTexture(texture, resizeType);
                    }
                    var dirArr = TextureUtility.CreateTexture2DArray(dir, isLinear: true, mipChain: false);
                    var dirPath = Path.Combine(dstDirPath, $"{LightmapType.Directional.ToFileName()}-{index}.asset");
                    if (dirArr != null)
                    {
                        KanikamaSceneUtility.CreateOrReplaceAsset(ref dirArr, dirPath);
                    }
                }
            }
        }
    }
}
