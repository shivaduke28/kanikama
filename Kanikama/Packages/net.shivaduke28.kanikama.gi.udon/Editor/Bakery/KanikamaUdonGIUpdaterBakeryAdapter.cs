using System.Linq;
using Kanikama.Core;
using Kanikama.Core.Editor;
using Kanikama.Core.Editor.Util;
using Kanikama.GI.Bakery.Baking.Editor;
using Kanikama.GI.Baking.Editor;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Udon.Editor.Bakery
{
    internal sealed class KanikamaUdonGIUpdaterBakeryAdapter : KanikamaWindow.IGUIDrawer
    {
        KanikamaUdonGIUpdater kanikamaUdonGIUpdater;
        SerializedObject serializedObject;

        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Runtime, () => new KanikamaUdonGIUpdaterBakeryAdapter(), 101);
        }

        KanikamaUdonGIUpdaterBakeryAdapter()
        {
            Load();
        }

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
        }

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

            var lightmapArrays = serializedObject.FindProperty("lightmapArrays");
            var directionalLightmapArrays = serializedObject.FindProperty("directionalLightmapArrays");
            var sliceCount = serializedObject.FindProperty("sliceCount");

            var arrayStorage = settingAsset.Setting.LightmapArrayStorage;
            var lightmapArrayList = arrayStorage.LightmapArrays;

            var lights = lightmapArrayList.Where(x => x.Type == BakeryLightmapType.Light).OrderBy(x => x.Index).ToArray();
            var directionals = lightmapArrayList.Where(x => x.Type == BakeryLightmapType.Directional).OrderBy(x => x.Index).ToArray();
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

            serializedObject.ApplyModifiedProperties();
            UdonSharpEditorUtility.CopyProxyToUdon(kanikamaUdonGIUpdater);
        }

        void KanikamaWindow.IGUIDrawer.Draw()
        {
            EditorGUILayout.LabelField($"{nameof(KanikamaUdonGIUpdater)} (Udon) (Bakery)", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                kanikamaUdonGIUpdater = (KanikamaUdonGIUpdater) EditorGUILayout.ObjectField("Provider",
                    kanikamaUdonGIUpdater, typeof(KanikamaUdonGIUpdater), true);

                if (kanikamaUdonGIUpdater == null)
                {
                    EditorGUILayout.HelpBox($"{nameof(KanikamaUdonGIUpdater)} is not found.", MessageType.Warning);
                }
                else
                {
                    if (KanikamaGUI.Button($"Setup by {nameof(BakeryBakingSettingAsset)} asset"))
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
