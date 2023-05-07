using System.Linq;
using Kanikama.Core;
using Kanikama.Core.Editor;
using Kanikama.GI.Bakery.Baking.Editor;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Runtime.Bakery.Editor
{
    [CustomEditor(typeof(KanikamaBakeryGIUpdater))]
    public sealed class KanikamaBakeryGIUpdaterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button($"Setup by {nameof(BakeryBakingSettingAsset)} asset"))
            {
                Setup();
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
            var arrayStorage = settingAsset.Setting.LightmapArrayStorage;
            var lightmapArrayList = arrayStorage.LightmapArrays;

            var lights = lightmapArrayList.Where(x => x.Type == BakeryLightmapType.Light).OrderBy(x => x.Index).ToArray();
            var directionals = lightmapArrayList.Where(x => x.Type == BakeryLightmapType.Directional).OrderBy(x => x.Index).ToArray();

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
    }
}
