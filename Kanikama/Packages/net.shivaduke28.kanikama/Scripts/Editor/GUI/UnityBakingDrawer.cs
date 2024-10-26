using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Editor.LTC;
using Kanikama.Impl;
using Kanikama.Utility;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.GUI
{
    public sealed class UnityBakingDrawer : KanikamaWindow.IGUIDrawer
    {
        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Baking, () => new UnityBakingDrawer(), 0);
        }

        SceneAsset sceneAsset;
        KanikamaManager kanikamaManager;
        UnityBakingSettingAsset bakingSettingAsset;
        bool isRunning;
        CancellationTokenSource cancellationTokenSource;
        SerializedObject serializedObject;


        UnityBakingDrawer()
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
            kanikamaManager = GameObjectHelper.FindObjectOfType<KanikamaManager>();
            serializedObject = new SerializedObject(kanikamaManager);

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
                kanikamaManager = GameObjectHelper.FindObjectOfType<KanikamaManager>();
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

            // if (!kanikamaManager.Validate())
            // {
            //     EditorGUILayout.HelpBox($"{nameof(KanikamaBakeTargetDescriptor)} has invalid null fields.", MessageType.Error);
            //     return;
            // }

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

            if (KanikamaGUI.Button("Create Assets") && ValidateAndLoadOnFail())
            {
                UnityBakingPipelineRunner.CreateAssets(kanikamaManager, new SceneAssetData(sceneAsset));
            }

            if (KanikamaGUI.Button($"Setup by {nameof(UnityBakingSetting)} asset"))
            {
                Setup();
            }
            if (KanikamaGUI.Button("Set KanikamaGI Receivers"))
            {
                SetupReceivers();
            }

            // if (KanikamaGUI.Button("Bake LTC") && ValidateAndLoadOnFail())
            // {
            //     cancellationTokenSource?.Cancel();
            //     cancellationTokenSource?.Dispose();
            //     cancellationTokenSource = new CancellationTokenSource();
            //     var __ = UnityLTCBakingPipeline.BakeAsync(new UnityLTCBakingPipeline.Parameter(
            //         new SceneAssetData(sceneAsset),
            //         bakingSettingAsset.Setting,
            //         kanikamaManager.GetLTCMonitors().ToList()
            //     ), cancellationTokenSource.Token);
            // }

            // if (KanikamaGUI.Button("Create LTC Assets") && ValidateAndLoadOnFail())
            // {
            //     UnityLTCBakingPipeline.CreateAssets(kanikamaManager.GetLTCMonitors().ToList(), bakingSettingAsset.Setting);
            // }
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

            if (settingAsset.Setting.AssetStorage.LightmapStorage.TryGet(UnityLTCBakingPipeline.LightmapKey, out var ltcVisibilityMapList))
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
