using System;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Bakery.Editor.LTC;
using Kanikama.Components;
using Kanikama.Editor;
using Kanikama.Editor.GUI;
using UnityEditor;
using UnityEngine;
using GameObjectUtility = Kanikama.Editor.GameObjectUtility;

namespace Kanikama.Bakery.Editor
{
    public sealed class BakeryBakingDrawer : KanikamaWindow.IGUIDrawer
    {
        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Baking, () => new BakeryBakingDrawer(), 10);
        }

        SceneAsset sceneAsset;
        KanikamaManager descriptor;
        BakeryBakingSettingAsset bakingSettingAsset;
        bool isRunning;
        CancellationTokenSource cancellationTokenSource;


        BakeryBakingDrawer()
        {
            Load();
        }

        void Load()
        {
            if (!SceneAssetData.TryFindFromActiveScene(out var sceneAssetData))
            {
                sceneAsset = null;
                bakingSettingAsset = null;
                descriptor = null;
                return;
            }

            sceneAsset = sceneAssetData.Asset;
            descriptor = GameObjectUtility.FindObjectOfType<KanikamaManager>();
            if (BakeryBakingSettingAsset.TryFind(sceneAsset, out var asset))
            {
                bakingSettingAsset = asset;
            }
            else
            {
                bakingSettingAsset = null;
            }
        }

        void KanikamaWindow.IGUIDrawer.OnLoadActiveScene() => Load();

        void KanikamaWindow.IGUIDrawer.Draw()
        {
            EditorGUILayout.LabelField("Bakery", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                if (isRunning)
                {
                    EditorGUILayout.HelpBox("Pipeline can be canceled from Bakery Progress Bar.", MessageType.Info);
                }
                else
                {
                    DrawScene();
                }
            }
        }

        void DrawScene()
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                sceneAsset = (SceneAsset) EditorGUILayout.ObjectField("Scene", sceneAsset, typeof(SceneAsset), false);
            }


            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (descriptor == null)
            {
                descriptor = GameObjectUtility.FindObjectOfType<KanikamaManager>();
            }

            descriptor = (KanikamaManager) EditorGUILayout.ObjectField("Scene Descriptor", descriptor, typeof(KanikamaManager), true);

            bakingSettingAsset =
                (BakeryBakingSettingAsset) EditorGUILayout.ObjectField("Settings", bakingSettingAsset, typeof(BakeryBakingSettingAsset), false);

            if (sceneAsset == null)
            {
                EditorGUILayout.HelpBox("The active Scene is not saved as an asset.", MessageType.Warning);
                return;
            }

            if (descriptor == null)
            {
                EditorGUILayout.HelpBox("BakingSceneDescriptor is not found.", MessageType.Warning);
                return;
            }

            // if (!descriptor.Validate())
            // {
            //     EditorGUILayout.HelpBox($"{nameof(KanikamaManager)} has invalid null fields.", MessageType.Error);
            //     return;
            // }

            if (bakingSettingAsset == null)
            {
                if (KanikamaGUI.Button("Load/Create Settings Asset"))
                {
                    bakingSettingAsset = BakeryBakingSettingAsset.FindOrCreate(sceneAsset);
                }
                EditorGUILayout.HelpBox("Create Kanikama Settings Asset.", MessageType.Warning);
                return;
            }

            if (KanikamaGUI.Button("Bake static") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource = new CancellationTokenSource();
                var _ = BakeStaticAsync(descriptor, new SceneAssetData(sceneAsset), cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Bake Kanikama") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource = new CancellationTokenSource();
                var _ = BakeKanikamaAsync(descriptor, bakingSettingAsset.Setting, cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Create Assets") && ValidateAndLoadOnFail())
            {
                BakeryBakingPipelineRunner.CreateAssets(descriptor, new SceneAssetData(sceneAsset));
            }

            // if (KanikamaGUI.Button("Bake LTC") && ValidateAndLoadOnFail())
            // {
            //     cancellationTokenSource?.Cancel();
            //     cancellationTokenSource?.Dispose();
            //     cancellationTokenSource = new CancellationTokenSource();
            //     var _ = BakeLTCAsync(cancellationTokenSource.Token);
            // }

            // if (KanikamaGUI.Button("Create LTC Assets") && ValidateAndLoadOnFail())
            // {
            //     BakeryLTCBakingPipeline.CreateAssets(descriptor.GetLTCMonitors(), bakingSettingAsset.Setting);
            // }
        }

        async Task BakeKanikamaAsync(KanikamaManager bakingDescriptor, BakeryBakingSetting setting, CancellationToken cancellationToken)
        {
            try
            {
                isRunning = true;
                await BakeryBakingPipelineRunner.BakeAsync(bakingDescriptor, setting, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                isRunning = false;
            }
        }

        async Task BakeStaticAsync(KanikamaManager bakingDescriptor, SceneAssetData sceneAssetData, CancellationToken cancellationToken)
        {
            try
            {
                isRunning = true;
                await BakeryBakingPipelineRunner.BakeStaticAsync(bakingDescriptor, sceneAssetData, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                isRunning = false;
            }
        }

        // async Task BakeLTCAsync(CancellationToken cancellationToken)
        // {
        //     try
        //     {
        //         isRunning = true;
        //         await BakeryLTCBakingPipeline.BakeAsync(new BakeryLTCBakingPipeline.Parameter(
        //             new SceneAssetData(sceneAsset),
        //             bakingSettingAsset.Setting,
        //             descriptor.GetLTCMonitors()
        //         ), cancellationToken);
        //     }
        //     catch (OperationCanceledException)
        //     {
        //         throw;
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogException(e);
        //     }
        //     finally
        //     {
        //         isRunning = false;
        //     }
        // }

        bool ValidateAndLoadOnFail()
        {
            var result = descriptor != null
                && SceneAssetData.TryFindFromActiveScene(out var sceneAssetData)
                && sceneAssetData.Asset == sceneAsset;

            if (!result)
            {
                Load();
            }
            return result;
        }
    }
}
