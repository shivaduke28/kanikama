using System.Linq;
using Editor.Application;
using Kanikama.Application.Impl;
using Kanikama.Editor.Baking;
using Kanikama.Editor.Baking.GUI;
using Kanikama.Editor.Baking.LTC;
using Kanikama.Utility;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Application.Editor
{
    internal sealed class KanikamaGIUpdaterDrawer : KanikamaWindow.IGUIDrawer
    {
        KanikamaRuntimeGIUpdater giUpdater;
        UnityBakingSettingAsset bakingSettingAsset;
        SerializedObject serializedObject;

        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Runtime, () => new KanikamaGIUpdaterDrawer(), 1);
        }

        KanikamaGIUpdaterDrawer()
        {
            Load();
        }

        void Load()
        {
            giUpdater = Object.FindObjectOfType<KanikamaRuntimeGIUpdater>();
            if (giUpdater != null)
            {
                serializedObject = new SerializedObject(giUpdater);
            }

            if (!SceneAssetData.TryFindFromActiveScene(out var sceneAssetData))
            {
                bakingSettingAsset = null;
                return;
            }

            if (UnityBakingSettingAsset.TryFind(sceneAssetData.Asset, out var asset))
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
                var ltcVisibilityMap = serializedObject.FindProperty("_Udon_LTC_VisibilityMap");
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
            EditorGUILayout.LabelField($"{nameof(KanikamaRuntimeGIUpdater)} (Unity)", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                giUpdater = (KanikamaRuntimeGIUpdater) EditorGUILayout.ObjectField("Scene Descriptor",
                    giUpdater, typeof(KanikamaRuntimeGIUpdater), true);

                if (giUpdater == null)
                {
                    EditorGUILayout.HelpBox($"{nameof(KanikamaRuntimeGIUpdater)} is not found.", MessageType.Warning);
                }
                else if (bakingSettingAsset == null)
                {
                    EditorGUILayout.HelpBox($"{nameof(UnityBakingSettingAsset)} is not found.", MessageType.Warning);
                }
                else
                {
                    if (KanikamaGUI.Button($"Setup by {nameof(UnityBakingSetting)} asset"))
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
