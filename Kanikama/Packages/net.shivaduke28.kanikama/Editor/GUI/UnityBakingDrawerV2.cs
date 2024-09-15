using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        KanikamaManager manager;
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
                manager = null;
                bakingSettingAsset = null;
                return;
            }

            sceneAsset = sceneAssetData.Asset;
            manager = GameObjectHelper.FindObjectOfType<KanikamaManager>();
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

            if (manager == null)
            {
                manager = GameObjectHelper.FindObjectOfType<KanikamaManager>();
            }

            manager = (KanikamaManager) EditorGUILayout.ObjectField("Scene Descriptor", manager, typeof(KanikamaManager), true);
            bakingSettingAsset =
                (UnityBakingSettingAsset) EditorGUILayout.ObjectField("Settings", bakingSettingAsset, typeof(UnityBakingSettingAsset), false);

            if (sceneAsset == null)
            {
                EditorGUILayout.HelpBox("The active Scene is not saved as an asset.", MessageType.Warning);
                return;
            }

            if (manager == null)
            {
                EditorGUILayout.HelpBox($"{nameof(KanikamaManager)}  is not found.", MessageType.Warning);
                return;
            }

            if (!manager.Validate())
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
                _ = BakeStaticAsync(manager, new SceneAssetData(sceneAsset), cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Bake Kanikama") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource = new CancellationTokenSource();
                _ = BakeKanikamaAsync(manager, new SceneAssetData(sceneAsset), cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Create Assets") && ValidateAndLoadOnFail())
            {
                CreateAssets(manager, new SceneAssetData(sceneAsset));
            }

            if (KanikamaGUI.Button("Bake LTC") && ValidateAndLoadOnFail())
            {
                // TODO: impl
                // cancellationTokenSource?.Cancel();
                // cancellationTokenSource?.Dispose();
                // cancellationTokenSource = new CancellationTokenSource();
                // var __ = UnityLtcBakingPipeline.BakeAsync(new UnityLtcBakingPipeline.Parameter(
                //     new SceneAssetData(sceneAsset),
                //     bakingSettingAsset.Setting,
                //     manager.GetLTCMonitors().ToList()
                // ), cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Create LTC Assets") && ValidateAndLoadOnFail())
            {
                // TODO: impl
                // UnityLtcBakingPipeline.CreateAssets(manager.GetLTCMonitors().ToList(), bakingSettingAsset.Setting);
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

        async Task BakeStaticAsync(KanikamaManager bakingDescriptor, SceneAssetData sceneAssetData, CancellationToken cancellationToken)
        {
            try
            {
                isRunning = true;
                var commands = CreateCommands(bakingDescriptor);
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

        static void CreateAssets(KanikamaManager kanikamaManager, SceneAssetData sceneAssetData)
        {
            var handles = CreateHandles(kanikamaManager);
            var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var setting = settingAsset.Setting;
            UnityBakingPipeline.CreateAssets(handles.Select(h => h.Id).ToArray(), setting);
        }


        bool ValidateAndLoadOnFail()
        {
            var result = manager != null;
            result = result && SceneAssetData.TryFindFromActiveScene(out var sceneAssetData) && sceneAssetData.Asset == sceneAsset;

            if (!result)
            {
                Load();
            }
            return result;
        }

        static List<IBakeTargetHandle> CreateHandles(KanikamaManager bakingDescriptor)
        {
            var bakeTargets = bakingDescriptor.GetLightSources();
            var handles = bakeTargets.Select(x => new BakeTargetHandleV2<LightSourceV2>(x)).Cast<IBakeTargetHandle>().ToList();
            handles.AddRange(bakingDescriptor.GetLightSourceGroups().SelectMany(GetElementHandles));
            return handles;
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
    }
}
