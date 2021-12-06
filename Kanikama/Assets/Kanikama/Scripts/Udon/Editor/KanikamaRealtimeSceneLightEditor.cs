using Kanikama.Baking;
using Kanikama.Editor;
using System.Linq;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Udon.Editor
{
    [CustomEditor(typeof(KanikamaRealtimeSceneLight))]
    public class KanikamaRealtimeSceneLightEditor : UnityEditor.Editor
    {
        UdonSharpBehaviour proxy;
        SerializedProperty weightsProperty;

        void OnEnable()
        {
            if (target == null) return;
            proxy = (UdonSharpBehaviour)target;
            weightsProperty = serializedObject.FindProperty("weights");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpEditorUtility.IsProxyBehaviour(proxy))
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();
                if (GUILayout.Button($"Reset weights"))
                {
                    ResetWeights();
                }
            }
            else
            {
                if (UdonSharpGUI.DrawConvertToUdonBehaviourButton(target))
                {
                    return;
                }
            }
        }

        void ResetWeights()
        {
            var lightmapCount = 0;
            var sceneAsset = KanikamaEditorUtility.GetActiveSceneAsset();
            if (sceneAsset != null)
            {
                var kanikamaSettings = KanikamaSettings.FindSettings(sceneAsset);
                if (kanikamaSettings != null)
                {
                    var lightmapArray = kanikamaSettings.bakedAsset.kanikamaMapArrays.FirstOrDefault();
                    if (lightmapArray != null)
                    {
                        lightmapCount = lightmapArray.depth;
                    }
                }
            }

            weightsProperty.arraySize = lightmapCount;
            for (var i = 0; i < lightmapCount; i++)
            {
                var prop = weightsProperty.GetArrayElementAtIndex(i);
                prop.floatValue = 1f;
            }

            serializedObject.ApplyModifiedProperties();
            UdonSharpEditorUtility.CopyProxyToUdon(proxy);
        }
    }
}