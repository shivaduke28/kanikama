using System.Linq;
using Kanikama.Application.Impl;
using Kanikama.Utility;
using Kanikama.Editor.Baking;
using Kanikama.Editor.Baking.Util;
using Kanikama.Baking.Editor;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Application.Editor
{
    internal sealed class KanikamaGIUpdaterDrawer : KanikamaWindow.IGUIDrawer
    {
        KanikamaRuntimeGIUpdater giUpdater;
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
            var arrayStorage = settingAsset.Setting.LightmapArrayStorage;
            var lightmapArrayList = arrayStorage.LightmapArrays;

            var lights = lightmapArrayList.Where(x => x.Type == UnityLightmapType.Light).OrderBy(x => x.Index).ToArray();
            var directionals = lightmapArrayList.Where(x => x.Type == UnityLightmapType.Directional).OrderBy(x => x.Index).ToArray();

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
            serializedObject.ApplyModifiedProperties();
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
                else
                {
                    if (KanikamaGUI.Button($"Setup by {nameof(UnityBakingSetting)} asset"))
                    {
                        Setup();
                    }
                }
                if (KanikamaGUI.Button("Load Active Scene"))
                {
                    Load();
                }
            }
        }
    }
}
