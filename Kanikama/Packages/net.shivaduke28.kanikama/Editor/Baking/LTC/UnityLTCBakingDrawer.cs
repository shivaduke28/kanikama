﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kanikama.Baking;
using Kanikama.Baking.LTC;
using Kanikama.Editor.Baking.GUI;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.Baking.LTC
{
    public class UnityLTCBakingDrawer : KanikamaWindow.IGUIDrawer
    {
        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Baking, () => new UnityLTCBakingDrawer(), 10);
        }

        SceneAsset sceneAsset;
        ILTCDescriptor descriptor;
        UnityLTCBakingSettingAsset settingAsset;
        bool isRunning;
        CancellationTokenSource cancellationTokenSource;


        UnityLTCBakingDrawer()
        {
            Load();
        }

        void Load()
        {
            if (!SceneAssetData.TryFindFromActiveScene(out var sceneAssetData))
            {
                sceneAsset = null;
                descriptor = null;
                settingAsset = null;
                return;
            }

            sceneAsset = sceneAssetData.Asset;
            descriptor = GameObjectHelper.FindObjectOfType<ILTCDescriptor>();
            if (UnityLTCBakingSettingAsset.TryFind(sceneAsset, out var asset))
            {
                settingAsset = asset;
            }
            else
            {
                settingAsset = null;
            }
        }

        void KanikamaWindow.IGUIDrawer.Draw()
        {
            EditorGUILayout.LabelField("Unity LTC", EditorStyles.boldLabel);
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

        void DrawScene()
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                sceneAsset = (SceneAsset) EditorGUILayout.ObjectField("Scene", sceneAsset, typeof(SceneAsset), false);
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (descriptor == null || (descriptor is Object sceneDescriptorObject && sceneDescriptorObject == null))
            {
                descriptor = GameObjectHelper.FindObjectOfType<ILTCDescriptor>();
            }

            if (descriptor is Object obj)
            {
                descriptor = (ILTCDescriptor) EditorGUILayout.ObjectField("LTC Descriptor", obj, typeof(MonoBehaviour), true);
            }

            settingAsset =
                (UnityLTCBakingSettingAsset) EditorGUILayout.ObjectField("LTC Settings", settingAsset, typeof(UnityLTCBakingSettingAsset), false);

            if (sceneAsset == null)
            {
                EditorGUILayout.HelpBox("The active Scene is not saved as an asset.", MessageType.Warning);
                return;
            }

            if (descriptor == null)
            {
                EditorGUILayout.HelpBox("LTC Descriptor is not found.", MessageType.Warning);
                return;
            }

            if (settingAsset == null)
            {
                if (KanikamaGUI.Button("Create Settings Asset"))
                {
                    settingAsset = UnityLTCBakingSettingAsset.FindOrCreate(sceneAsset);
                }
                EditorGUILayout.HelpBox("Create Kanikama LTC Settings Asset.", MessageType.Warning);
                return;
            }

            if (KanikamaGUI.Button("Bake LTC") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = new CancellationTokenSource();
                var (gridFiberHandles, monitorHandles) = ToHandles(descriptor);
                var __ = UnityLTCBakingPipeline.BakeAsync(new UnityLTCBakingPipeline.Context(
                    new SceneAssetData(sceneAsset),
                    gridFiberHandles,
                    monitorHandles,
                    new UnityLightmapper(),
                    settingAsset.Setting
                ), cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Create Assets") && ValidateAndLoadOnFail())
            {
                var (gridFiberHandles, monitorHandles) = ToHandles(descriptor);
                UnityLTCBakingPipeline.CreateAssets(gridFiberHandles, monitorHandles, settingAsset.Setting);
            }
        }

        static (IList<IBakeTargetHandle> gridFiberHandles, IList<IBakeTargetHandle> monitorHandles) ToHandles(ILTCDescriptor descriptor)
        {
            var monitors = descriptor.GetMonitors();
            var monitorGroup = descriptor.GetMonitorGroup();
            return (
                gridFiberHandles: monitorGroup.GetAll().Select((_, i) => new BakeTargetGroupElementHandle<BakeTargetGroup>(monitorGroup, i))
                    .Cast<IBakeTargetHandle>()
                    .ToList(),
                monitorHandles: monitors
                    .Select(x => new BakeTargetGroupHandle<BakeTargetGroup>(x))
                    .Cast<IBakeTargetHandle>()
                    .ToList());
        }

        void KanikamaWindow.IGUIDrawer.OnLoadActiveScene() => Load();

        bool ValidateAndLoadOnFail()
        {
            var result = descriptor != null;
            result = result && SceneAssetData.TryFindFromActiveScene(out var sceneAssetData) && sceneAssetData.Asset == sceneAsset;

            if (!result)
            {
                Load();
            }
            return result;
        }
    }
}
