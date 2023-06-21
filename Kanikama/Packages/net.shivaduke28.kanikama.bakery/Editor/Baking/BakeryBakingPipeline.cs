using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Utility;
using Kanikama.Editor.Baking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanikama.Bakery.Editor.Baking
{
    public static class BakeryBakingPipeline
    {
        public const string LightmapArrayKey = "KanikamaBakery";

        public sealed class Parameter
        {
            public SceneAssetData SceneAssetData { get; }
            public BakeryBakingSetting Setting { get; }
            public IList<IBakingCommand> Commands { get; }

            public Parameter(SceneAssetData sceneAssetData,
                BakeryBakingSetting setting, IList<IBakingCommand> commands)
            {
                SceneAssetData = sceneAssetData;
                Setting = setting;
                Commands = commands;
            }
        }

        public sealed class Context
        {
            public SceneAssetData SceneAssetData { get; } // copied
            public BakeryLightmapper Lightmapper { get; }
            public BakeryBakingSetting Setting { get; }
            public BakerySceneGIContext SceneGIContext { get; }

            public Context(SceneAssetData sceneAssetData, BakeryLightmapper lightmapper,
                BakeryBakingSetting setting, BakerySceneGIContext sceneGIContext)
            {
                SceneAssetData = sceneAssetData;
                Lightmapper = lightmapper;
                Setting = setting;
                SceneGIContext = sceneGIContext;
            }
        }

        public static async Task BakeAsync(Parameter parameter, CancellationToken cancellationToken)
        {
            Debug.LogFormat(KanikamaDebug.Format, "Bakery pipeline start");
            using (var copiedScene = CopiedSceneAsset.Create(parameter.SceneAssetData, true))
            {
                try
                {
                    IOUtility.CreateFolderIfNecessary(parameter.Setting.OutputAssetDirPath);
                    // open the copied scene
                    EditorSceneManager.OpenScene(copiedScene.SceneAssetData.Path);
                    var giContext = BakerySceneGIContext.GetContext();
                    giContext.TurnOff();
                    var context = new Context(copiedScene.SceneAssetData, new BakeryLightmapper(), parameter.Setting, giContext);

                    // initialize all light source handles **after** the copied scene is opened
                    var guid = copiedScene.SceneAssetData.Guid;
                    foreach (var command in parameter.Commands)
                    {
                        command.Initialize(guid);
                    }

                    // save scene（maybe delete this...)
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

                    foreach (var command in parameter.Commands)
                    {
                        await command.RunAsync(context, cancellationToken);
                    }

                    // NOTE: A SettingAsset object may be destroyed while awaiting baking for some reason...
                    // So we use Setting class and update asset after baking done.
                    var settingAssets = BakeryBakingSettingAsset.FindOrCreate(parameter.SceneAssetData.Asset);
                    settingAssets.Setting = parameter.Setting;
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
                    EditorSceneManager.OpenScene(parameter.SceneAssetData.Path);
                }
            }
        }

        public static List<Lightmap> CopyLightmaps(List<Lightmap> src, string dstDir, string id)
        {
            var dst = new List<Lightmap>(src.Count);
            foreach (var lightmap in src)
            {
                var dstPath = Path.Combine(dstDir, CopiedLightmapName(lightmap, id));
                AssetDatabase.CopyAsset(lightmap.Path, dstPath);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(dstPath);
                var copied = new Lightmap(lightmap.Type, texture, dstPath, lightmap.Index);
                dst.Add(copied);
            }
            return dst;
        }

        public static async Task BakeStaticAsync(Parameter parameter, CancellationToken cancellationToken)
        {
            Debug.LogFormat(KanikamaDebug.Format, "Bakery pipeline non Kanikama start");
            var guid = AssetDatabase.AssetPathToGUID(parameter.SceneAssetData.Path);

            foreach (var command in parameter.Commands)
            {
                command.Initialize(guid);
            }

            try
            {
                var lightmapper = new BakeryLightmapper();
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
                foreach (var command in parameter.Commands)
                {
                    command.Clear();
                }
            }
        }

        static string CopiedLightmapName(Lightmap bakedLightmap, string id)
        {
            var path = bakedLightmap.Path;
            var fileName = Path.GetFileName(path);
            var ext = Path.GetExtension(fileName);
            return $"{bakedLightmap.Type}-{bakedLightmap.Index}-{id}{ext}";
        }

        public static void CreateAssets(IEnumerable<IBakeTargetHandle> bakeTargetHandles, BakeryBakingSetting setting)
        {
            Debug.LogFormat(KanikamaDebug.Format, $"create assets (resize type: {setting.TextureResizeType})");
            var dstDirPath = setting.OutputAssetDirPath;
            var resizeType = setting.TextureResizeType;
            var lightmapStorage = setting.AssetStorage.LightmapStorage;
            IOUtility.CreateFolderIfNecessary(dstDirPath);

            // check lightmaps are stored in storage
            var allLightmaps = new List<Lightmap>();
            var hasError = false;
            foreach (var handle in bakeTargetHandles)
            {
                if (lightmapStorage.TryGet(handle.Id, out var lm))
                {
                    allLightmaps.AddRange(lm);
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

            var lightmaps = allLightmaps.Where(lm => lm.Type == BakeryLightmap.Light || lm.Type == BakeryLightmap.L0).ToArray();
            var directionalMaps = allLightmaps.Where(lm => lm.Type == BakeryLightmap.Directional || lm.Type == BakeryLightmap.L1).ToArray();
            var maxIndex = lightmaps.Max(lightmap => lightmap.Index);

            var output = new List<LightmapArray>();
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
                    var lightPath = Path.Combine(dstDirPath, $"{BakeryLightmap.Light}-{index}.asset");
                    if (lightArr != null)
                    {
                        IOUtility.CreateOrReplaceAsset(ref lightArr, lightPath);
                        output.Add(new LightmapArray(BakeryLightmap.Light, lightArr, lightPath, index));
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
                    var dirPath = Path.Combine(dstDirPath, $"{BakeryLightmap.Directional}-{index}.asset");
                    if (dirArr != null)
                    {
                        IOUtility.CreateOrReplaceAsset(ref dirArr, dirPath);
                        output.Add(new LightmapArray(BakeryLightmap.Directional, dirArr, dirPath, index));
                        Debug.LogFormat(KanikamaDebug.Format, $"create asset: {dirPath}");
                    }
                }
            }
            setting.AssetStorage.LightmapArrayStorage.AddOrUpdate(LightmapArrayKey, output, "KanikamaBakery");

            Debug.LogFormat(KanikamaDebug.Format, "done");
        }
    }
}
