using System.Linq;
using System.Threading;
using Kanikama.Baking.Experimental.LTC;
using Kanikama.Baking.Experimental.LTC.Impl;
using Kanikama.Editor.Baking.GUI;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.Baking.Experimental.LTC
{
    public class UnityLTCBakingDrawer : KanikamaWindow.IGUIDrawer
    {
        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Baking, () => new UnityLTCBakingDrawer(), 10);
        }

        SceneAsset sceneAsset;
        KanikamaLTCDescriptor descriptor;
        UnityBakingSettingAsset settingAsset;
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
            descriptor = GameObject.FindObjectOfType<KanikamaLTCDescriptor>();
            if (UnityBakingSettingAsset.TryFind(sceneAsset, out var asset))
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
                descriptor = GameObject.FindObjectOfType<KanikamaLTCDescriptor>();
            }

            if (descriptor is Object obj)
            {
                descriptor = (KanikamaLTCDescriptor) EditorGUILayout.ObjectField("LTC Descriptor", obj, typeof(KanikamaLTCDescriptor), true);
            }

            settingAsset =
                (UnityBakingSettingAsset) EditorGUILayout.ObjectField("LTC Settings", settingAsset, typeof(UnityBakingSettingAsset), false);

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
                    settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAsset);
                }
                EditorGUILayout.HelpBox("Create Kanikama LTC Settings Asset.", MessageType.Warning);
                return;
            }

            if (KanikamaGUI.Button("Bake LTC") && ValidateAndLoadOnFail())
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = new CancellationTokenSource();
                var __ = UnityLTCBakingPipeline.BakeAsync(new UnityLTCBakingPipeline.Parameter(
                    new SceneAssetData(sceneAsset),
                    settingAsset.Setting,
                    descriptor.GetMonitors().ToList()
                ), cancellationTokenSource.Token);
            }

            if (KanikamaGUI.Button("Create Assets") && ValidateAndLoadOnFail())
            {
                UnityLTCBakingPipeline.CreateAssets(descriptor.GetMonitors().ToList(), settingAsset.Setting);
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
