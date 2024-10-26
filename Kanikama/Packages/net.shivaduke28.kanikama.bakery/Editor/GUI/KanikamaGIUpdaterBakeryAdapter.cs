using System.Linq;
using Kanikama.Bakery.Editor.LTC;
using Kanikama.Editor;
using Kanikama.Editor.GUI;
using Kanikama.Utility;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Bakery.Editor.GUI
{
    internal sealed class KanikamaGIUpdaterBakeryAdapter : KanikamaWindow.IGUIDrawer
    {
        KanikamaManager giUpdater;
        SerializedObject serializedObject;
        BakeryBakingSettingAsset bakingSettingAsset;

        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Runtime, () => new KanikamaGIUpdaterBakeryAdapter(), 2);
        }

        KanikamaGIUpdaterBakeryAdapter()
        {
            Load();
        }

        void Load()
        {
            giUpdater = Object.FindObjectOfType<KanikamaManager>();
            if (giUpdater != null)
            {
                serializedObject = new SerializedObject(giUpdater);
            }
            if (!SceneAssetData.TryFindFromActiveScene(out var sceneAssetData))
            {
                bakingSettingAsset = null;
                return;
            }

            if (BakeryBakingSettingAsset.TryFind(sceneAssetData.Asset, out var asset))
            {
                bakingSettingAsset = asset;
            }
            else
            {
                bakingSettingAsset = null;
            }
        }

        void KanikamaWindow.IGUIDrawer.OnLoadActiveScene() => Load();


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

            if (settingAsset.Setting.AssetStorage.LightmapStorage.TryGet(BakeryLTCBakingPipeline.LightmapKey, out var ltcVisibilityMapList))
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
            Undo.RecordObject(giUpdater, "Setup Receivers");
            var receivers = serializedObject.FindProperty("receivers");
            var renderers = RendererCollector.CollectKanikamaReceivers();
            receivers.arraySize = renderers.Length;
            for (var i = 0; i < renderers.Length; i++)
            {
                receivers.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            }
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(giUpdater);
        }

        void KanikamaWindow.IGUIDrawer.Draw()
        {
            EditorGUILayout.LabelField($"{nameof(KanikamaManager)} (Bakery)", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                giUpdater = (KanikamaManager) EditorGUILayout.ObjectField("Scene Descriptor", giUpdater, typeof(KanikamaManager), true);

                if (giUpdater == null)
                {
                    EditorGUILayout.HelpBox($"{nameof(KanikamaManager)} is not found.", MessageType.Warning);
                }
                // else if (!giUpdater.Validate())
                // {
                //     EditorGUILayout.HelpBox($"{nameof(KanikamaManager)} has invalid null fields.", MessageType.Error);
                // }
                else if (bakingSettingAsset == null)
                {
                    EditorGUILayout.HelpBox($"{nameof(BakeryBakingSettingAsset)} is not found.", MessageType.Warning);
                }
                else
                {
                    if (KanikamaGUI.Button($"Setup by {nameof(BakeryBakingSettingAsset)} asset"))
                    {
                        Setup();
                    }
                    if (KanikamaGUI.Button("Set KanikamaGI Receivers"))
                    {
                        SetupReceivers();
                    }
                }
            }
        }
    }
}
