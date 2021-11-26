using System.Linq;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Udon.Editor
{
    [CustomEditor(typeof(KanikamaColorCollector))]
    public class KanikamaColorCollectorEditor : UnityEditor.Editor
    {
        UdonSharpBehaviour proxy;
        SerializedProperty lightsProperty;
        SerializedProperty emissiveRenderersProperty;
        SerializedProperty kanikamaCamerasProperty;
        SerializedProperty isAmbientEnableProperty;
        SerializedProperty colorsProperty;

        bool colorsFold;

        void OnEnable()
        {
            if (target == null) return;
            proxy = (UdonSharpBehaviour)target;
            lightsProperty = serializedObject.FindProperty("lights");
            emissiveRenderersProperty = serializedObject.FindProperty("emissiveRenderers");
            kanikamaCamerasProperty = serializedObject.FindProperty("kanikamaCameras");
            isAmbientEnableProperty = serializedObject.FindProperty("isAmbientEnable");
            colorsProperty = serializedObject.FindProperty("colors");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpEditorUtility.IsProxyBehaviour(proxy))
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                if (Application.isPlaying)
                {
                    colorsFold = EditorGUILayout.BeginFoldoutHeaderGroup(colorsFold, "Colors");
                    if (colorsFold)
                    {
                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            var colorCount = colorsProperty.arraySize;
                            for (var i = 0; i < colorCount; i++)
                            {
                                var colorProp = colorsProperty.GetArrayElementAtIndex(i);
                                var value = colorProp.vector4Value;
                                var color = new Color(value.x, value.y, value.z);
                                EditorGUILayout.ColorField(new GUIContent($"Element {i}"), color, false, false, true);
                            }
                        }
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }

                EditorGUILayout.Space();
                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                {
                    if (GUILayout.Button($"Setup by {nameof(KanikamaSceneDescriptor)}"))
                    {
                        Setup();
                    }
                }
                serializedObject.ApplyModifiedProperties();
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
            var sceneDescriptor = FindObjectOfType<KanikamaSceneDescriptor>();
            if (sceneDescriptor == null)
            {
                Debug.LogError($"[Kanikama] {nameof(sceneDescriptor)} object is not found.");
                return;
            }

            var lights = sceneDescriptor.KanikamaLights;
            lightsProperty.ClearArray();
            lightsProperty.arraySize = lights.Count;
            for (var i = 0; i < lights.Count; i++)
            {
                var prop = lightsProperty.GetArrayElementAtIndex(i);
                prop.objectReferenceValue = lights[i].GetSource();
            }

            var rendererGroups = sceneDescriptor.KanikamaRendererGroups;
            emissiveRenderersProperty.ClearArray();
            emissiveRenderersProperty.arraySize = rendererGroups.Count;
            for (var i = 0; i < rendererGroups.Count; i++)
            {
                var prop = emissiveRenderersProperty.GetArrayElementAtIndex(i);
                prop.objectReferenceValue = rendererGroups[i].GetSource();
            }

            var kanikamaCameras = sceneDescriptor.KanikamaLightSourceGroups
                .Where(x => x is KanikamaMonitorController)
                .Select(x => ((KanikamaMonitorController)x).Camera.GetComponent<KanikamaCamera>())
                .ToArray();
            kanikamaCamerasProperty.ClearArray();
            kanikamaCamerasProperty.arraySize = kanikamaCameras.Length;
            for (var i = 0; i < kanikamaCameras.Length; i++)
            {
                var prop = kanikamaCamerasProperty.GetArrayElementAtIndex(i);
                prop.objectReferenceValue = kanikamaCameras[i];
            }

            serializedObject.ApplyModifiedProperties();
            UdonSharpEditorUtility.CopyProxyToUdon(proxy);
        }
    }
}