using Kanikama.Baking;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Udon.Editor
{
    [CustomEditor(typeof(KanikamaMapArrayProvider))]
    public class KanikamaMapArrayProviderEditor : UnityEditor.Editor
    {
        UdonSharpBehaviour proxy;
        SerializedProperty lightmapArraysProperty;
        SerializedProperty directionalLightmapArraysProperty;
        SerializedProperty lightmapCountProperty;

        void OnEnable()
        {
            if (target == null) return;
            proxy = (UdonSharpBehaviour)target;
            lightmapArraysProperty = serializedObject.FindProperty("lightmapArrays");
            directionalLightmapArraysProperty = serializedObject.FindProperty("directionalLightmapArrays");
            lightmapCountProperty = serializedObject.FindProperty("lightmapCount");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpEditorUtility.IsProxyBehaviour(proxy))
            {
                base.OnInspectorGUI();

                EditorGUILayout.Space();
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                EditorGUILayout.Space();
                if (GUILayout.Button($"Setup by {nameof(KanikamaSettings)} asset"))
                {
                    Setup();
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                if (UdonSharpGUI.DrawConvertToUdonBehaviourButton(target))
                {
                    return;
                }
            }
        }

        void Setup()
        {
            var sceneAsset = KanikamaEditorUtility.GetActiveSceneAsset();
            if (sceneAsset == null)
            {
                Debug.LogError("[Kanikama] This scene is not saved yet.");
                return;
            }
            var kanikamaSettings = KanikamaSettings.FindSettings(sceneAsset);
            if (kanikamaSettings == null)
            {
                Debug.LogError("[Kanikama] KanikamaSettings asset is not found.");
                return;
            }

            var bakedAsset = kanikamaSettings.bakedAsset;
            var arrayCount = bakedAsset.kanikamaMapArrays.Count;
            lightmapArraysProperty.arraySize = arrayCount;
            for (var i = 0; i < arrayCount; i++)
            {
                var prop = lightmapArraysProperty.GetArrayElementAtIndex(i);
                prop.objectReferenceValue = bakedAsset.kanikamaMapArrays[i];
            }

            var dirArrayCount = kanikamaSettings.directionalMode ? bakedAsset.kanikamaDirectionalMapArrays.Count : 0;
            directionalLightmapArraysProperty.arraySize = dirArrayCount;

            for (var i = 0; i < dirArrayCount; i++)
            {
                var prop = directionalLightmapArraysProperty.GetArrayElementAtIndex(i);
                prop.objectReferenceValue = bakedAsset.kanikamaDirectionalMapArrays[i];
            }

            var sliceCount = arrayCount > 0 ? bakedAsset.kanikamaMapArrays[0].depth : 0;
            lightmapCountProperty.intValue = sliceCount;

            serializedObject.ApplyModifiedProperties();
            UdonSharpEditorUtility.CopyProxyToUdon(proxy);
        }
    }
}