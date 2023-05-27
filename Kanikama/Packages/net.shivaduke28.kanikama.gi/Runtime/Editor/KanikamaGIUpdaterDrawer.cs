using System.Linq;
using Kanikama.Core;
using Kanikama.Core.Editor;
using Kanikama.Core.Editor.Util;
using Kanikama.GI.Baking.Editor;
using Kanikama.GI.Runtime.Impl;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Runtime.Editor
{
    internal sealed class KanikamaGIUpdaterDrawer : KanikamaGIWindow.IGUIDrawer
    {
        KanikamaGIUpdater giUpdater;
        SerializedObject serializedObject;

        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaGIWindow.AddDrawer(KanikamaGIWindow.Category.Runtime, () => new KanikamaGIUpdaterDrawer(), 1);
        }

        KanikamaGIUpdaterDrawer()
        {
            Load();
        }

        void Load()
        {
            giUpdater = Object.FindObjectOfType<KanikamaGIUpdater>();
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

        void KanikamaGIWindow.IGUIDrawer.Draw()
        {
            GUILayout.Label($"{nameof(KanikamaGIUpdater)} (Unity)", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                giUpdater = (KanikamaGIUpdater) EditorGUILayout.ObjectField("Scene Descriptor",
                    giUpdater, typeof(KanikamaGIUpdater), true);

                if (giUpdater == null)
                {
                    EditorGUILayout.HelpBox($"{nameof(KanikamaGIUpdater)} is not found.", MessageType.Warning);
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
