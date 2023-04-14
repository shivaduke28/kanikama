using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core;
using Kanikama.Core.Editor;
using Kanikama.GI.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Bakery.Editor
{
    public sealed class BakingPipeline
    {
        public sealed class Context
        {
            public SceneAssetData SceneAssetData { get; }
            public List<IBakeTargetHandle> BakeTargetHandles { get; }
            public BakeryLightmapper Lightmapper { get; }

            public Context(SceneAssetData sceneAssetData, List<IBakeTargetHandle> bakeTargetHandles, BakeryLightmapper lightmapper)
            {
                SceneAssetData = sceneAssetData;
                BakeTargetHandles = bakeTargetHandles;
                Lightmapper = lightmapper;
            }
        }

        public async Task BakeAsync(Context context, CancellationToken cancellationToken)
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

                    var dstDir = $"{context.SceneAssetData.LightingAssetDirectoryPath}_kanikama-temp";
                    KanikamaSceneUtility.CreateFolderIfNecessary(dstDir);

                    // 元のシーンに対して取得する（Contextに入れた方がよさそう)
                    var assets = BakeryBakingSettingAsset.FindOrCreate(context.SceneAssetData.Asset);
                    var lightmapper = context.Lightmapper;

                    // TODO: Lightmapperのパラメータ指定があるはず
                    foreach (var handle in bakeTargetHandles)
                    {
                        handle.TurnOn();
                        await lightmapper.BakeAsync(cancellationToken);
                        handle.TurnOff();

                        // TODO: ここがBakery使用になるはず
                        var baked = KanikamaBakeryUtility.GetLightmaps(lightmapper.OutputDirPath, copiedSceneHandle.SceneAssetData.Asset.name);
                        var copied = new List<BakeryLightmap>();
                        Copy(baked, copied, handle.Id);

                        assets.AddOrUpdate(handle.Id, copied);
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
            }

            void Copy(List<BakeryLightmap> src, List<BakeryLightmap> dst, string key)
            {
                // TODO: 実装
            }
        }
    }
}
