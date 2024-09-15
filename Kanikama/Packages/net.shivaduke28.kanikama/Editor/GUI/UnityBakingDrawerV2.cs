using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Components;
using Kanikama.Utility;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.GUI
{
    public sealed class UnityBakingDrawerV2 : KanikamaWindow.IGUIDrawer
    {
        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Baking, () => new UnityBakingDrawerV2(), 0);
        }

        SceneAsset sceneAsset;
        KanikamaManager kanikamaManager;
        SerializedObject serializedKanikamaManager;
        UnityBakingSettingAsset bakingSettingAsset;
        bool isRunning;
        CancellationTokenSource cancellationTokenSource;

        UnityBakingDrawerV2()
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
            if (kanikamaManager != null)
            {
                serializedKanikamaManager = new SerializedObject(kanikamaManager);
            }
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
            EditorGUILayout.LabelField("Unity V2", EditorStyles.boldLabel);
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
                if (kanikamaManager != null)
                {
                    serializedKanikamaManager = new SerializedObject(kanikamaManager);
                }
            }

            kanikamaManager = (KanikamaManager) EditorGUILayout.ObjectField(nameof(KanikamaManager), kanikamaManager, typeof(KanikamaManager), true);
            bakingSettingAsset =
                (UnityBakingSettingAsset) EditorGUILayout.ObjectField("Settings", bakingSettingAsset, typeof(UnityBakingSettingAsset), false);

            if (sceneAsset == null)
            {
                EditorGUILayout.HelpBox("The active Scene is not saved as an asset.", MessageType.Warning);
                return;
            }

            if (kanikamaManager == null)
            {
                EditorGUILayout.HelpBox($"{nameof(KanikamaManager)}  is not found.", MessageType.Warning);
                return;
            }

            if (!kanikamaManager.Validate())
            {
                EditorGUILayout.HelpBox($"{nameof(KanikamaManager)} has invalid null fields.", MessageType.Error);
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
                _ = BakeStaticAsync(kanikamaManager, new SceneAssetData(sceneAsset), cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Bake Kanikama") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource = new CancellationTokenSource();
                _ = BakeKanikamaAsync(kanikamaManager, new SceneAssetData(sceneAsset), cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Create Assets") && ValidateAndLoadOnFail())
            {
                CreateKanikamaAssets(kanikamaManager, new SceneAssetData(sceneAsset));
            }

            if (KanikamaGUI.Button("Bake LTC") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource = new CancellationTokenSource();
                _ = BakeLtcAsync(kanikamaManager, new SceneAssetData(sceneAsset), cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Create LTC Assets") && ValidateAndLoadOnFail())
            {
                CreateLtcAssets(kanikamaManager, new SceneAssetData(sceneAsset), bakingSettingAsset.Setting);
            }

            if (KanikamaGUI.Button($"Set baked assets to {nameof(KanikamaManager)} by {nameof(UnityBakingSetting)} asset"))
            {
                Setup();
            }
            if (KanikamaGUI.Button($"Set KanikamaGI Receivers to {nameof(KanikamaManager)}"))
            {
                SetupReceivers();
            }
        }

        async Task BakeKanikamaAsync(KanikamaManager kanikamaManager, SceneAssetData sceneAssetData, CancellationToken cancellationToken)
        {
            try
            {
                isRunning = true;
                var commands = CreateCommands(kanikamaManager);
                var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
                var parameter = new UnityBakingPipeline.Parameter(sceneAssetData, settingAsset.Setting, commands);
                await UnityBakingPipeline.BakeAsync(parameter, cancellationToken);
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

        async Task BakeStaticAsync(KanikamaManager manager, SceneAssetData sceneAssetData, CancellationToken cancellationToken)
        {
            try
            {
                isRunning = true;
                var commands = CreateCommands(manager);
                var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
                var parameter = new UnityBakingPipeline.Parameter(sceneAssetData, settingAsset.Setting, commands);
                await UnityBakingPipeline.BakeStaticAsync(parameter, cancellationToken);
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

        static void CreateKanikamaAssets(KanikamaManager kanikamaManager, SceneAssetData sceneAssetData)
        {
            var bakeTargets = kanikamaManager.GetLightSources();
            var handles = bakeTargets.Select(x => new BakeTargetHandleV2<LightSourceV2>(x)).Cast<IBakeTargetHandle>().ToList();
            handles.AddRange(kanikamaManager.GetLightSourceGroups().SelectMany(GetElementHandles));


            var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var setting = settingAsset.Setting;
            UnityBakingPipeline.CreateAssets(handles.Select(h => h.Id).ToArray(), setting);
        }

        async Task BakeLtcAsync(KanikamaManager manager, SceneAssetData sceneAssetData, CancellationToken token)
        {
            try
            {
                isRunning = true;
                var commands = manager.GetLtcMonitors()
                    .Select(x => new UnityLtcBakingCommand(new BakeTargetHandleV2<KanikamaLtcMonitor>(x)))
                    .ToArray();
                await UnityLtcBakingPipeline.BakeAsync(new UnityLtcBakingPipeline.Parameter(
                        sceneAssetData,
                        bakingSettingAsset.Setting,
                        commands),
                    token);
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

        static void CreateLtcAssets(KanikamaManager manager, SceneAssetData sceneAssetData, UnityBakingSetting bakingSetting)
        {
            // FIXME: Creating assets should use not commands but handles.
            var commands = manager.GetLtcMonitors()
                .Select(x => new UnityLtcBakingCommand(new BakeTargetHandleV2<KanikamaLtcMonitor>(x)))
                .ToArray();
            UnityLtcBakingPipeline.CreateAssets(new UnityLtcBakingPipeline.Parameter(
                sceneAssetData,
                bakingSetting,
                commands));
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

        static IEnumerable<IBakeTargetHandle> GetElementHandles(LightSourceGroupV2 g)
        {
            return g.GetAll().Select((_, i) => new BakeTargetGroupElementHandleV2<LightSourceGroupV2>(g, i));
        }

        static IUnityBakingCommand[] CreateCommands(KanikamaManager bakingDescriptor)
        {
            var commands = new List<IUnityBakingCommand>();

            commands.AddRange(bakingDescriptor.GetLightSources().Select(x => new UnityBakingCommand(new BakeTargetHandleV2<LightSourceV2>(x))));
            commands.AddRange(bakingDescriptor.GetLightSourceGroups().SelectMany(GetElementHandles).Select(h => new UnityBakingCommand(h)));
            return commands.ToArray();
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

            var lightmapArrays = serializedKanikamaManager.FindProperty("lightmapArrays");
            lightmapArrays.arraySize = lights.Length;
            for (var i = 0; i < lights.Length; i++)
            {
                lightmapArrays.GetArrayElementAtIndex(i).objectReferenceValue = lights[i].Texture;
            }
            var directionalLightmapArrays = serializedKanikamaManager.FindProperty("directionalLightmapArrays");
            directionalLightmapArrays.arraySize = directionals.Length;
            for (var i = 0; i < directionals.Length; i++)
            {
                directionalLightmapArrays.GetArrayElementAtIndex(i).objectReferenceValue = directionals[i].Texture;
            }

            if (settingAsset.Setting.AssetStorage.LightmapStorage.TryGet(UnityLtcBakingPipeline.LightmapKey, out var ltcVisibilityMapList))
            {
                var ltcVisibilityMap = serializedKanikamaManager.FindProperty("ltcVisibilityMaps");
                ltcVisibilityMap.arraySize = lights.Length;
                for (var i = 0; i < lights.Length; i++)
                {
                    ltcVisibilityMap.GetArrayElementAtIndex(i).objectReferenceValue = ltcVisibilityMapList[i].Texture;
                }
            }

            serializedKanikamaManager.ApplyModifiedProperties();
        }

        void SetupReceivers()
        {
            Undo.RecordObject(kanikamaManager, "Setup Receivers");
            var receivers = serializedKanikamaManager.FindProperty("receivers");
            var renderers = RendererCollector.CollectKanikamaReceivers();
            receivers.arraySize = renderers.Length;
            for (var i = 0; i < renderers.Length; i++)
            {
                receivers.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            }
            serializedKanikamaManager.ApplyModifiedProperties();
            EditorUtility.SetDirty(kanikamaManager);
        }
    }
}
