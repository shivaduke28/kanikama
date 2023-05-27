using System.Linq;
using Kanikama.Core;
using Kanikama.Core.Editor;
using Kanikama.Core.Editor.Util;
using Kanikama.GI.Bakery.Baking.Editor;
using Kanikama.GI.Baking.Editor;
using Kanikama.GI.Baking.Editor;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Udon.Editor.Bakery
{
    internal sealed class KanikamaMapArrayProviderBakeryAdapter : KanikamaGIWindow.IGUIDrawer
    {
        KanikamaMapArrayProvider kanikamaMapArrayProvider;
        SerializedObject serializedObject;

        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaGIWindow.AddDrawer(KanikamaGIWindow.Category.Runtime, () => new KanikamaMapArrayProviderBakeryAdapter(), 101);
        }

        KanikamaMapArrayProviderBakeryAdapter()
        {
            Load();
        }

        void Load()
        {
            kanikamaMapArrayProvider = Object.FindObjectOfType<KanikamaMapArrayProvider>();
            if (kanikamaMapArrayProvider != null)
            {
                serializedObject = new SerializedObject(kanikamaMapArrayProvider);
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
            UdonSharpEditorUtility.CopyProxyToUdon(kanikamaMapArrayProvider);
        }

        void KanikamaGIWindow.IGUIDrawer.Draw()
        {
            GUILayout.Label($"{nameof(KanikamaMapArrayProvider)} (Udon) (Bakery)", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                kanikamaMapArrayProvider = (KanikamaMapArrayProvider) EditorGUILayout.ObjectField("Provider",
                    kanikamaMapArrayProvider, typeof(KanikamaMapArrayProvider), true);

                if (kanikamaMapArrayProvider == null)
                {
                    EditorGUILayout.HelpBox($"{nameof(KanikamaMapArrayProvider)} is not found.", MessageType.Warning);
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
