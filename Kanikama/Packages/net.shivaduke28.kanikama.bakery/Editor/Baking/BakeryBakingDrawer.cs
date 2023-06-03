﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Editor.Baking;
using Kanikama.Baking;
using Kanikama.Editor.Baking.GUI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.Bakery.Editor.Baking.GUI
{
    public sealed class BakeryBakingDrawer : KanikamaWindow.IGUIDrawer
    {
        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Baking, () => new BakeryBakingDrawer(), 2);
        }

        SceneAsset sceneAsset;
        IBakingDescriptor sceneDescriptor;
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
                sceneDescriptor = null;
                return;
            }

            sceneAsset = sceneAssetData.Asset;
            sceneDescriptor = GameObjectHelper.FindObjectOfType<IBakingDescriptor>();
            if (BakeryBakingSettingAsset.TryFind(sceneAsset, out var asset))
            {
                bakingSettingAsset = asset;
            }
            else
            {
                bakingSettingAsset = null;
            }
        }

        void KanikamaWindow.IGUIDrawer.Draw()
        {
            EditorGUILayout.LabelField("Bakery", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                if (isRunning)
                {
                    if (KanikamaGUI.Button("Cancel"))
                    {
                        cancellationTokenSource.Cancel();
                        cancellationTokenSource.Dispose();
                        cancellationTokenSource = null;
                    }
                }
                else
                {
                    DrawScene();

                    if (KanikamaGUI.Button("Load Active Scene"))
                    {
                        Load();
                    }
                }
            }
        }

        void DrawScene()
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                sceneAsset = (SceneAsset) EditorGUILayout.ObjectField("Scene", sceneAsset, typeof(SceneAsset), false);
            }


            if (sceneDescriptor == null)
            {
                sceneDescriptor = GameObjectHelper.FindObjectOfType<IBakingDescriptor>();
            }
            
            if (sceneDescriptor is Object sceneDescriptorObject)
            {
                sceneDescriptor = (IBakingDescriptor) EditorGUILayout.ObjectField("Scene Descriptor", sceneDescriptorObject, typeof(MonoBehaviour), true);
            }

            bakingSettingAsset =
                (BakeryBakingSettingAsset) EditorGUILayout.ObjectField("Settings", bakingSettingAsset, typeof(BakeryBakingSettingAsset), false);

            if (sceneAsset == null)
            {
                EditorGUILayout.HelpBox("The active Scene is not saved as an asset.", MessageType.Warning);
                return;
            }

            if (sceneDescriptor == null)
            {
                EditorGUILayout.HelpBox("BakingSceneDescriptor is not found.", MessageType.Warning);
                return;
            }

            if (bakingSettingAsset == null)
            {
                if (KanikamaGUI.Button("Load/Create Settings Asset"))
                {
                    bakingSettingAsset = BakeryBakingSettingAsset.FindOrCreate(sceneAsset);
                }
                EditorGUILayout.HelpBox("Create Kanikama Settings Asset.", MessageType.Warning);
                return;
            }

            if (KanikamaGUI.Button("Bake Kanikama") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource = new CancellationTokenSource();
                var _ = BakeKanikamaAsync(sceneDescriptor, bakingSettingAsset.Setting, cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Bake static") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource = new CancellationTokenSource();
                var _ = BakeStaticAsync(sceneDescriptor, new SceneAssetData(sceneAsset), cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Create Assets") && ValidateAndLoadOnFail())
            {
                BakeryBakingPipelineRunner.CreateAssets(sceneDescriptor, new SceneAssetData(sceneAsset));
            }
        }

        async Task BakeKanikamaAsync(IBakingDescriptor bakingDescriptor, BakeryBakingSetting setting, CancellationToken cancellationToken)
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

        async Task BakeStaticAsync(IBakingDescriptor bakingDescriptor, SceneAssetData sceneAssetData, CancellationToken cancellationToken)
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

        bool ValidateAndLoadOnFail()
        {
            var result = sceneDescriptor != null;
            result = result && SceneAssetData.TryFindFromActiveScene(out var sceneAssetData);
            result = result && sceneAssetData.Asset == sceneAsset;

            if (!result)
            {
                Load();
            }
            return result;
        }
    }
}