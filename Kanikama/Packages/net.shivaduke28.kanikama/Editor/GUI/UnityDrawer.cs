using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Editor.Utility;
using Kanikama.Utility;
using UnityEditor;
using UnityEngine;
using GameObjectUtility = Kanikama.Editor.Utility.GameObjectUtility;

namespace Kanikama.Editor.GUI
{
    public sealed class UnityDrawer : KanikamaWindow.IGUIDrawer
    {
        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Unity, () => new UnityDrawer(), 0);
        }

        SceneAsset sceneAsset;
        KanikamaManager kanikamaManager;
        UnityBakingSettingAsset bakingSettingAsset;
        bool isRunning;
        CancellationTokenSource cancellationTokenSource;

        UnityDrawer()
        {
            Load();
        }

        void Load()
        {
            if (!SceneAssetData.TryFindFromActiveScene(out var sceneAssetData))
            {
                sceneAsset = null;
                kanikamaManager = null;
                bakingSettingAsset = null;
                return;
            }

            sceneAsset = sceneAssetData.Asset;
            kanikamaManager = GameObjectUtility.FindObjectOfType<KanikamaManager>();

            if (UnityBakingSettingAsset.TryFind(sceneAsset, out var asset))
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
            EditorGUILayout.LabelField("Unity", EditorStyles.boldLabel);
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
                }
            }
        }

        void KanikamaWindow.IGUIDrawer.OnLoadActiveScene() => Load();

        void DrawScene()
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                sceneAsset = (SceneAsset) EditorGUILayout.ObjectField("Scene", sceneAsset, typeof(SceneAsset), false);
            }

            if (kanikamaManager == null)
            {
                kanikamaManager = GameObjectUtility.FindObjectOfType<KanikamaManager>();
            }

            kanikamaManager = (KanikamaManager) EditorGUILayout.ObjectField("Kanikama Manager", kanikamaManager, typeof(KanikamaManager), true);
            bakingSettingAsset =
                (UnityBakingSettingAsset) EditorGUILayout.ObjectField("Settings", bakingSettingAsset, typeof(UnityBakingSettingAsset), false);

            if (sceneAsset == null)
            {
                EditorGUILayout.HelpBox("The active Scene is not saved as an asset.", MessageType.Warning);
                return;
            }

            if (kanikamaManager == null)
            {
                EditorGUILayout.HelpBox($"{nameof(KanikamaManager)} is not found.", MessageType.Warning);
                return;
            }

            if (bakingSettingAsset == null)
            {
                if (KanikamaGUI.Button("Create Settings Asset"))
                {
                    bakingSettingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAsset);
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
                var _ = BakeKanikamaAsync(kanikamaManager, new SceneAssetData(sceneAsset), cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Create Kanikama Assets") && ValidateAndLoadOnFail())
            {
                UnityBakingPipelineRunner.CreateAssets(kanikamaManager, new SceneAssetData(sceneAsset));
            }

            EditorGUILayout.Space();

            if (KanikamaGUI.Button("Bake LTC") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = new CancellationTokenSource();
                _ = UnityLtcBakingPipeline.BakeAsync(new UnityLtcBakingPipeline.Parameter(
                    new SceneAssetData(sceneAsset),
                    bakingSettingAsset.Setting,
                    kanikamaManager.GetLtcMonitors().ToList()
                ), cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Create LTC Assets") && ValidateAndLoadOnFail())
            {
                UnityLtcBakingPipeline.CreateAssets(kanikamaManager.GetLtcMonitors().ToList(), bakingSettingAsset.Setting);
            }

            EditorGUILayout.Space();

            if (KanikamaGUI.Button($"Set Assets to {nameof(KanikamaManager)} from {nameof(UnityBakingSetting)} asset"))
            {
                Setup();
            }
            if (KanikamaGUI.Button($"Set KanikamaGI Receivers to {nameof(KanikamaManager)}"))
            {
                SetupReceivers();
            }
        }

        async Task BakeKanikamaAsync(KanikamaManager bakingDescriptor, SceneAssetData sceneAssetData, CancellationToken cancellationToken)
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

        async Task BakeStaticAsync(KanikamaManager bakingDescriptor, SceneAssetData sceneAssetData, CancellationToken cancellationToken)
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
            var result = kanikamaManager != null;
            result = result && SceneAssetData.TryFindFromActiveScene(out var sceneAssetData) && sceneAssetData.Asset == sceneAsset;

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
            if (!UnityBakingSettingAsset.TryFind(sceneAssetData.Asset, out var settingAsset))
            {
                Debug.LogErrorFormat(KanikamaDebug.Format, $"{nameof(UnityBakingSettingAsset)} is not found.");
                return;
            }
            if (!settingAsset.Setting.AssetStorage.LightmapArrayStorage.TryGet(UnityBakingPipeline.LightmapArrayKey, out var lightmapArrayList)) return;

            var lights = lightmapArrayList.Where(x => x.Type == UnityLightmap.Light).OrderBy(x => x.Index).ToArray();
            var directionals = lightmapArrayList.Where(x => x.Type == UnityLightmap.Directional).OrderBy(x => x.Index).ToArray();

            var serializedObject = new SerializedObject(kanikamaManager);
            var lightmapArrays = serializedObject.FindProperty("lightmapArrays");
            lightmapArrays.arraySize = lights.Length;
            for (var i = 0; i < lights.Length; i++)
            {
                lightmapArrays.GetArrayElementAtIndex(i).objectReferenceValue = lights[i].Texture;
            }

            var sliceCount = serializedObject.FindProperty("sliceCount");
            sliceCount.intValue = lights.Length > 0 ? lights[0].Texture.depth : 0;

            var directionalLightmapArrays = serializedObject.FindProperty("directionalLightmapArrays");
            directionalLightmapArrays.arraySize = directionals.Length;
            for (var i = 0; i < directionals.Length; i++)
            {
                directionalLightmapArrays.GetArrayElementAtIndex(i).objectReferenceValue = directionals[i].Texture;
            }

            if (settingAsset.Setting.AssetStorage.LightmapStorage.TryGet(UnityLtcBakingPipeline.LightmapKey, out var ltcVisibilityMapList))
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
            var serializedObject = new SerializedObject(kanikamaManager);
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
