using System.Threading;
using Baking.Experimental.LTC.Impl;
using Kanikama.Bakery.Editor.Baking;
using Kanikama.Editor.Baking.GUI;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.Baking.LTC
{
    public class BakeryLTCBakingDrawer : KanikamaWindow.IGUIDrawer
    {
        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Baking, () => new BakeryLTCBakingDrawer(), 11);
        }

        SceneAsset sceneAsset;
        KanikamaBakeryLTCDescriptor descriptor;
        BakeryBakingSettingAsset settingAsset;
        bool isRunning;
        CancellationTokenSource cancellationTokenSource;


        BakeryLTCBakingDrawer()
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
            descriptor = GameObjectHelper.FindObjectOfType<KanikamaBakeryLTCDescriptor>();
            if (BakeryBakingSettingAsset.TryFind(sceneAsset, out var asset))
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
            EditorGUILayout.LabelField("Bakery LTC", EditorStyles.boldLabel);
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
                descriptor = GameObjectHelper.FindObjectOfType<KanikamaBakeryLTCDescriptor>();
            }

            if (descriptor is Object obj)
            {
                descriptor = (KanikamaBakeryLTCDescriptor) EditorGUILayout.ObjectField("LTC Descriptor", obj, typeof(KanikamaBakeryLTCDescriptor), true);
            }

            settingAsset =
                (BakeryBakingSettingAsset) EditorGUILayout.ObjectField("LTC Settings", settingAsset, typeof(BakeryBakingSettingAsset), false);

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
                    settingAsset = BakeryBakingSettingAsset.FindOrCreate(sceneAsset);
                }
                EditorGUILayout.HelpBox("Create Kanikama LTC Settings Asset.", MessageType.Warning);
                return;
            }

            if (KanikamaGUI.Button("Bake LTC") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = new CancellationTokenSource();
                var __ = BakeryLTCBakingPipeline.BakeAsync(new BakeryLTCBakingPipeline.Parameter(
                    new SceneAssetData(sceneAsset),
                    settingAsset.Setting,
                    descriptor.GetMonitors()
                ), cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Create Assets") && ValidateAndLoadOnFail())
            {
                BakeryLTCBakingPipeline.CreateAssets(descriptor.GetMonitors(), settingAsset.Setting);
            }
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
