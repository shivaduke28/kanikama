using System.Linq;
using Editor.Application;
using Kanikama.Utility;
using Kanikama.Editor.Baking;
using Kanikama.Editor.Baking.GUI;
using Kanikama.Editor.Baking.LTC;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Udon.Editor
{
    internal sealed class KanikamaUdonGIUpdaterDrawer : KanikamaWindow.IGUIDrawer
    {
        KanikamaUdonGIUpdater kanikamaUdonGIUpdater;
        SerializedObject serializedObject;
        UnityBakingSettingAsset bakingSettingAsset;

        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Runtime, () => new KanikamaUdonGIUpdaterDrawer(), 100);
        }

        KanikamaUdonGIUpdaterDrawer()
        {
            Load();
        }

        void KanikamaWindow.IGUIDrawer.OnLoadActiveScene() => Load();

        void Load()
        {
            kanikamaUdonGIUpdater = Object.FindObjectOfType<KanikamaUdonGIUpdater>();
            if (kanikamaUdonGIUpdater != null)
            {
                serializedObject = new SerializedObject(kanikamaUdonGIUpdater);
            }
            else
            {
                serializedObject = null;
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

            Undo.RecordObject(kanikamaUdonGIUpdater, "Setup GI Updater");
            UdonSharpEditorUtility.CopyUdonToProxy(kanikamaUdonGIUpdater);
            var lightmapArrays = serializedObject.FindProperty("lightmapArrays");
            var directionalLightmapArrays = serializedObject.FindProperty("directionalLightmapArrays");
            var sliceCount = serializedObject.FindProperty("sliceCount");

            var arrayStorage = settingAsset.Setting.AssetStorage.LightmapArrayStorage;
            if (!settingAsset.Setting.AssetStorage.LightmapArrayStorage.TryGet(UnityBakingPipeline.LightmapArrayKey, out var lightmapArrayList)) return;

            var lights = lightmapArrayList.Where(x => x.Type == UnityLightmap.Light).OrderBy(x => x.Index).ToArray();
            var directionals = lightmapArrayList.Where(x => x.Type == UnityLightmap.Directional).OrderBy(x => x.Index).ToArray();
            lightmapArrays.arraySize = lights.Length;
            for (var i = 0; i < lights.Length; i++)
            {
                lightmapArrays.GetArrayElementAtIndex(i).objectReferenceValue = lights[i].Texture;
            }

            directionalLightmapArrays.arraySize = directionals.Length;
            for (var i = 0; i < directionals.Length; i++)
            {
                directionalLightmapArrays.GetArrayElementAtIndex(i).objectReferenceValue = directionals[i].Texture;
            }

            sliceCount.intValue = lights.Length > 0 ? lights[0].Texture.depth : 0;

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
            UdonSharpEditorUtility.CopyProxyToUdon(kanikamaUdonGIUpdater);
            EditorUtility.SetDirty(kanikamaUdonGIUpdater);
        }

        void SetupReceivers()
        {
            Undo.RecordObject(kanikamaUdonGIUpdater, "Setup Receivers");
            UdonSharpEditorUtility.CopyUdonToProxy(kanikamaUdonGIUpdater);
            var receivers = serializedObject.FindProperty("receivers");
            var renderers = RendererCollector.CollectKanikamaReceivers();
            receivers.arraySize = renderers.Length;
            for (var i = 0; i < renderers.Length; i++)
            {
                receivers.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            }
            serializedObject.ApplyModifiedProperties();
            UdonSharpEditorUtility.CopyProxyToUdon(kanikamaUdonGIUpdater);
            EditorUtility.SetDirty(kanikamaUdonGIUpdater);
        }

        void KanikamaWindow.IGUIDrawer.Draw()
        {
            EditorGUILayout.LabelField($"{nameof(KanikamaUdonGIUpdater)} (Udon)", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                kanikamaUdonGIUpdater = (KanikamaUdonGIUpdater) EditorGUILayout.ObjectField("Provider",
                    kanikamaUdonGIUpdater, typeof(KanikamaUdonGIUpdater), true);

                if (kanikamaUdonGIUpdater == null)
                {
                    EditorGUILayout.HelpBox($"{nameof(KanikamaUdonGIUpdater)} is not found.", MessageType.Warning);
                }
                else if (!kanikamaUdonGIUpdater.Validate())
                {
                    EditorGUILayout.HelpBox($"{nameof(KanikamaUdonGIUpdater)} has invalid null fields.", MessageType.Error);
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
