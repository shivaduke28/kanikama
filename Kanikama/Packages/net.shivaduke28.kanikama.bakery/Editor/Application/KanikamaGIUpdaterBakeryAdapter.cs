using System.Linq;
using Kanikama.Utility;
using Kanikama.Editor.Baking;
using Kanikama.Application.Impl;
using Kanikama.Bakery.Editor.Baking;
using Kanikama.Editor.Baking.GUI;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Bakery.Editor.Application
{
    internal sealed class KanikamaGIUpdaterBakeryAdapter : KanikamaWindow.IGUIDrawer
    {
        KanikamaRuntimeGIUpdater giUpdater;
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
            var lightmapArrayList = arrayStorage.GetAll();

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
            serializedObject.ApplyModifiedProperties();
        }

        void KanikamaWindow.IGUIDrawer.Draw()
        {
            EditorGUILayout.LabelField($"{nameof(KanikamaRuntimeGIUpdater)} (Bakery)", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                giUpdater = (KanikamaRuntimeGIUpdater) EditorGUILayout.ObjectField("Scene Descriptor", giUpdater, typeof(KanikamaRuntimeGIUpdater), true);

                if (giUpdater == null)
                {
                    EditorGUILayout.HelpBox($"{nameof(KanikamaRuntimeGIUpdater)} is not found.", MessageType.Warning);
                }
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
                }
            }
        }
    }
}
