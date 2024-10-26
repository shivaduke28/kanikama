using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Editor;
using Kanikama.Editor.GUI;
using Kanikama.Editor.Utility;
using Kanikama.Utility;
using UnityEditor;
using UnityEngine;
using GameObjectUtility = Kanikama.Editor.Utility.GameObjectUtility;

namespace Kanikama.Bakery.Editor.GUI
{
    public sealed class BakeryDrawer : KanikamaWindow.IGUIDrawer
    {
        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Bakery, () => new BakeryDrawer(), 10);
        }

        SceneAsset sceneAsset;
        KanikamaManager kanikamaManager;
        SerializedObject serializedObject;
        BakeryBakingSettingAsset bakingSettingAsset;
        bool isRunning;
        CancellationTokenSource cancellationTokenSource;


        BakeryDrawer()
        {
            Load();
        }

        void Load()
        {
            if (!SceneAssetData.TryFindFromActiveScene(out var sceneAssetData))
            {
                sceneAsset = null;
                bakingSettingAsset = null;
                kanikamaManager = null;
                return;
            }

            sceneAsset = sceneAssetData.Asset;
            kanikamaManager = GameObjectUtility.FindObjectOfType<KanikamaManager>();
            serializedObject = new SerializedObject(kanikamaManager);
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
            if (kanikamaManager == null)
            {
                kanikamaManager = GameObjectUtility.FindObjectOfType<KanikamaManager>();
            }

            kanikamaManager = (KanikamaManager) EditorGUILayout.ObjectField("Scene Descriptor", kanikamaManager, typeof(KanikamaManager), true);

            bakingSettingAsset =
                (BakeryBakingSettingAsset) EditorGUILayout.ObjectField("Settings", bakingSettingAsset, typeof(BakeryBakingSettingAsset), false);

            if (sceneAsset == null)
            {
                EditorGUILayout.HelpBox("The active Scene is not saved as an asset.", MessageType.Warning);
                return;
            }

            if (kanikamaManager == null)
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

            if (KanikamaGUI.Button("Bake static") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource = new CancellationTokenSource();
                var _ = BakeStaticAsync(kanikamaManager, new SceneAssetData(sceneAsset), cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Bake Kanikama") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource = new CancellationTokenSource();
                var _ = BakeKanikamaAsync(kanikamaManager, bakingSettingAsset.Setting, cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Create Assets") && ValidateAndLoadOnFail())
            {
                BakeryBakingPipelineRunner.CreateAssets(kanikamaManager, new SceneAssetData(sceneAsset));
            }

            EditorGUILayout.Space();

            if (KanikamaGUI.Button("Bake LTC") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = new CancellationTokenSource();
                _ = BakeLTCAsync(cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Create LTC Assets") && ValidateAndLoadOnFail())
            {
                BakeryLtcBakingPipeline.CreateAssets(kanikamaManager.GetLtcMonitors(), bakingSettingAsset.Setting);
            }

            EditorGUILayout.Space();
            if (KanikamaGUI.Button($"Setup by {nameof(BakeryBakingSettingAsset)} asset"))
            {
                Setup();
            }
            if (KanikamaGUI.Button("Set KanikamaGI Receivers"))
            {
                SetupReceivers();
            }
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

        async Task BakeLTCAsync(CancellationToken cancellationToken)
        {
            try
            {
                isRunning = true;
                await BakeryLtcBakingPipeline.BakeAsync(new BakeryLtcBakingPipeline.Parameter(
                    new SceneAssetData(sceneAsset),
                    bakingSettingAsset.Setting,
                    kanikamaManager.GetLtcMonitors()
                ), cancellationToken);
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
            var result = kanikamaManager != null
                && SceneAssetData.TryFindFromActiveScene(out var sceneAssetData)
                && sceneAssetData.Asset == sceneAsset;

            if (!result)
            {
                Load();
            }
            return result;
        }

        void Setup()
        {
            if (!SceneAssetData.TryFindFromActiveScene(out var sceneAssetData))
            {
                Debug.LogErrorFormat(KanikamaDebug.Format, "The current active scene is not saved as an asset.");
                return;
            }
            if (!BakeryBakingSettingAsset.TryFind(sceneAssetData.Asset, out var settingAsset))
            {
                Debug.LogErrorFormat(KanikamaDebug.Format, $"{nameof(BakeryBakingSettingAsset)} is not found.");
                return;
            }
            var arrayStorage = settingAsset.Setting.AssetStorage.LightmapArrayStorage;
            if (!arrayStorage.TryGet(BakeryBakingPipeline.LightmapArrayKey, out var lightmapArrayList)) return;

            var lights = lightmapArrayList.Where(x => x.Type == BakeryLightmap.Light).OrderBy(x => x.Index).ToArray();
            var directionals = lightmapArrayList.Where(x => x.Type == BakeryLightmap.Directional).OrderBy(x => x.Index).ToArray();

            var lightmapArrays = serializedObject.FindProperty("lightmapArrays");
            lightmapArrays.arraySize = lights.Length;
            for (var i = 0; i < lights.Length; i++)
            {
                lightmapArrays.GetArrayElementAtIndex(i).objectReferenceValue = lights[i].Texture;
            }
            var directionalLightmapArrays = serializedObject.FindProperty("directionalLightmapArrays");
            directionalLightmapArrays.arraySize = directionals.Length;
            for (var i = 0; i < directionals.Length; i++)
            {
                directionalLightmapArrays.GetArrayElementAtIndex(i).objectReferenceValue = directionals[i].Texture;
            }

            if (settingAsset.Setting.AssetStorage.LightmapStorage.TryGet(BakeryLtcBakingPipeline.LightmapKey, out var ltcVisibilityMapList))
            {
                var ltcVisibilityMap = serializedObject.FindProperty("ltcVisibilityMaps");
                ltcVisibilityMap.arraySize = lights.Length;
                for (var i = 0; i < lights.Length; i++)
                {
                    ltcVisibilityMap.GetArrayElementAtIndex(i).objectReferenceValue = ltcVisibilityMapList[i].Texture;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void SetupReceivers()
        {
            Undo.RecordObject(kanikamaManager, "Setup Receivers");
            var receivers = serializedObject.FindProperty("receivers");
            var renderers = RendererCollector.CollectKanikamaReceivers();
            receivers.arraySize = renderers.Length;
            for (var i = 0; i < renderers.Length; i++)
            {
                receivers.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            }
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(kanikamaManager);
        }
    }
}
