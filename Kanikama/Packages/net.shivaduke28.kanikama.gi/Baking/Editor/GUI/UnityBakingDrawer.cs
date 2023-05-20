using System;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core.Editor;
using Kanikama.GI.Baking;
using Kanikama.GI.Baking.Editor.GUI;
using Kanikama.GI.Baking.Impl;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Editor.GUI
{
    public sealed class UnityBakingDrawer : KanikamaGIWindow.IGUIDrawer
    {
        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaGIWindow.AddDrawer(KanikamaGIWindow.Category.Baking, "Unity", () => new UnityBakingDrawer(), 1);
        }

        SceneAsset sceneAsset;
        KanikamaSceneDescriptor sceneDescriptor;
        UnityBakingSettingAsset bakingSettingAsset;
        bool isRunning;
        CancellationTokenSource cancellationTokenSource;


        UnityBakingDrawer()
        {
            Load();
        }

        void Load()
        {
            if (!SceneAssetData.TryFindFromActiveScene(out var sceneAssetData))
            {
                sceneAsset = null;
                sceneDescriptor = null;
                bakingSettingAsset = null;
                return;
            }

            sceneAsset = sceneAssetData.Asset;
            sceneDescriptor = Object.FindObjectOfType<KanikamaSceneDescriptor>();
            if (UnityBakingSettingAsset.TryFind(sceneAsset, out var asset))
            {
                bakingSettingAsset = asset;
            }
            else
            {
                bakingSettingAsset = null;
            }
        }

        void KanikamaGIWindow.IGUIDrawer.Draw()
        {
            GUILayout.Label("Unity", EditorStyles.boldLabel);
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

        void DrawScene()
        {
            DrawLoadButton();
            using (new EditorGUI.DisabledGroupScope(true))
            {
                sceneAsset = (SceneAsset) EditorGUILayout.ObjectField("Scene", sceneAsset, typeof(SceneAsset), false);
            }

            if (sceneDescriptor == null)
            {
                sceneDescriptor = Object.FindObjectOfType<KanikamaSceneDescriptor>();
            }

            sceneDescriptor = (KanikamaSceneDescriptor) EditorGUILayout.ObjectField("Scene Descriptor",
                sceneDescriptor, typeof(KanikamaSceneDescriptor), true);
            bakingSettingAsset =
                (UnityBakingSettingAsset) EditorGUILayout.ObjectField("Settings", bakingSettingAsset, typeof(UnityBakingSettingAsset), false);

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
                if (GUILayout.Button("Create Settings Asset"))
                {
                    bakingSettingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAsset);
                }
                EditorGUILayout.HelpBox("Create Kanikama Settings Asset.", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Bake Kanikama") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource = new CancellationTokenSource();
                var _ = BakeKanikamaAsync(sceneDescriptor, new SceneAssetData(sceneAsset), cancellationTokenSource.Token);
            }

            if (GUILayout.Button("Bake static") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource = new CancellationTokenSource();
                var _ = BakeStaticAsync(sceneDescriptor, new SceneAssetData(sceneAsset), cancellationTokenSource.Token);
            }

            if (GUILayout.Button("Create Assets") && ValidateAndLoadOnFail())
            {
                UnityBakingPipelineRunner.CreateAssets(sceneDescriptor, new SceneAssetData(sceneAsset));
            }
        }

        void DrawLoadButton()
        {
            if (GUILayout.Button("Load Active Scene"))
            {
                Load();
            }
        }

        async Task BakeKanikamaAsync(IBakingDescriptor bakingDescriptor, SceneAssetData sceneAssetData, CancellationToken cancellationToken)
        {
            try
            {
                isRunning = true;
                await UnityBakingPipelineRunner.BakeAsync(bakingDescriptor, sceneAssetData, cancellationToken);
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
                await UnityBakingPipelineRunner.BakeStaticAsync(bakingDescriptor, sceneAssetData, cancellationToken);
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
