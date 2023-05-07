using System.Linq;
using Kanikama.Core;
using Kanikama.Core.Editor;
using Kanikama.GI.Editor;
using Kanikama.GI.Runtime.Impl;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Runtime.Editor
{
    [CustomEditor(typeof(KanikamaGIUpdater))]
    public class KanikamaGIUpdaterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button($"Setup by {nameof(UnityBakingSetting)} asset"))
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
    }
}
