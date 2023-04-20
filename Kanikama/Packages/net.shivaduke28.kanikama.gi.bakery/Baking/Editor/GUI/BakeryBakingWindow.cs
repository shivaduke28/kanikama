using System;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core.Editor;
using Kanikama.GI.Bakery.Editor;
using Kanikama.GI.Baking.Impl;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Baking.Editor.GUI
{
    public sealed class BakeryBakingWindow : EditorWindow
    {
        SceneAsset sceneAsset;
        KanikamaSceneDescriptor sceneDescriptor;
        BakeryBakingSettingAsset bakingSettingAsset;
        Vector2 scrollPosition = new Vector2(0, 0);
        bool isRunning;
        CancellationTokenSource cancellationTokenSource;

        [MenuItem("Window/Kanikama/Baking (Bakery)")]
        static void Initialize()
        {
            var window = GetWindow<BakeryBakingWindow>();
            window.Show();
        }

        void OnEnable()
        {
            titleContent.text = "Kanikama Baking (Bakery)";
            Load();
        }

        void Load()
        {
            if (!KanikamaSceneUtility.TryGetActiveSceneAsset(out var sceneAssetData))
            {
                sceneAsset = null;
                bakingSettingAsset = null;
                sceneDescriptor = null;
                return;
            }

            sceneAsset = sceneAssetData.Asset;
            sceneDescriptor = FindObjectOfType<KanikamaSceneDescriptor>();
            if (BakeryBakingSettingAsset.TryFind(sceneAsset, out var asset))
            {
                bakingSettingAsset = asset;
            }
            else
            {
                bakingSettingAsset = null;
            }
        }

        void OnGUI()
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scroll.scrollPosition;

                if (isRunning)
                {
                    if (GUILayout.Button("Cancel"))
                    {
                        cancellationTokenSource.Cancel();
                        cancellationTokenSource.Dispose();
                        cancellationTokenSource = null;
                    }
                }
                else
                {
                    DrawScene();
                }
            }
        }

        void DrawScene()
        {
            GUILayout.Label("Scene", EditorStyles.boldLabel);
            DrawLoadButton();

            using (new EditorGUI.DisabledGroupScope(true))
            {
                sceneAsset = (SceneAsset) EditorGUILayout.ObjectField("Scene", sceneAsset, typeof(SceneAsset), false);
            }

            if (sceneDescriptor == null)
            {
                sceneDescriptor = FindObjectOfType<KanikamaSceneDescriptor>();
            }

            sceneDescriptor = (KanikamaSceneDescriptor) EditorGUILayout.ObjectField("Scene Descriptor",
                sceneDescriptor, typeof(KanikamaSceneDescriptor), true);
            bakingSettingAsset =
                (BakeryBakingSettingAsset) EditorGUILayout.ObjectField("Settings", bakingSettingAsset, typeof(BakeryBakingSettingAsset), false);

            if (sceneAsset == null)
            {
                EditorGUILayout.HelpBox("The active Scene is not saved as an asset.", MessageType.Warning);
                return;
            }

            if (sceneDescriptor == null)
            {
                EditorGUILayout.HelpBox("KanikamaSceneDescriptor is not found.", MessageType.Warning);
                return;
            }

            if (bakingSettingAsset == null)
            {
                if (GUILayout.Button("Load/Create Settings Asset"))
                {
                    bakingSettingAsset = BakeryBakingSettingAsset.FindOrCreate(sceneAsset);
                }
                EditorGUILayout.HelpBox("Create Kanikama Settings Asset.", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Bake Kanikama") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource = new CancellationTokenSource();
                var _ = BakeKanikamaAsync(sceneDescriptor, bakingSettingAsset.Setting, cancellationTokenSource.Token);
            }

            if (GUILayout.Button("Bake non Kanikama") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource = new CancellationTokenSource();
                var _ = BakeNonKanikamaAsync(sceneDescriptor, KanikamaSceneUtility.ToAssetData(sceneAsset), cancellationTokenSource.Token);
            }

            if (GUILayout.Button("Create Assets") && ValidateAndLoadOnFail())
            {
                BakeryBakingPipeline.CreateAssets(bakingSettingAsset.Setting);
            }
        }

        void DrawLoadButton()
        {
            if (GUILayout.Button("Load Active Scene"))
            {
                Load();
            }
        }

        async Task BakeKanikamaAsync(IBakingDescriptor bakingDescriptor, BakeryBakingSetting setting, CancellationToken cancellationToken)
        {
            try
            {
                isRunning = true;
                await BakeryBakingPipelineRunner.RunAsync(bakingDescriptor, setting, cancellationToken);
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

        async Task BakeNonKanikamaAsync(IBakingDescriptor bakingDescriptor, SceneAssetData sceneAssetData, CancellationToken cancellationToken)
        {
            try
            {
                isRunning = true;
                await BakeryBakingPipelineRunner.RunWithoutKanikamaAsync(bakingDescriptor, sceneAssetData, cancellationToken);
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
            result = result && KanikamaSceneUtility.TryGetActiveSceneAsset(out var sceneAssetData);
            result = result && sceneAssetData.Asset == sceneAsset;

            if (!result)
            {
                Load();
            }
            return result;
        }
    }
}
