using Kanikama.EditorOnly;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using Kanikama.Editor;
using UdonSharp;

namespace Kanikama.Udon.Editor
{
    [CustomEditor(typeof(KanikamaCamera))]
    public class KanikamaCameraEditor : UnityEditor.Editor
    {
        UdonSharpBehaviour proxy;
        SerializedProperty aspectRatioProperty;
        SerializedProperty partitionTypeProperty;
        SerializedProperty colorsProperty;
        bool colorsFold;
        void OnEnable()
        {
            if (target == null) return;
            proxy = (UdonSharpBehaviour)target;
            aspectRatioProperty = serializedObject.FindProperty("aspectRatio");
            partitionTypeProperty = serializedObject.FindProperty("partitionType");
            colorsProperty = serializedObject.FindProperty("colors");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpEditorUtility.IsProxyBehaviour(proxy))
            {
                base.OnInspectorGUI();
                colorsFold = KanikamaEditorGUI.ArrayField(colorsProperty, colorsFold, true);

                EditorGUILayout.Space();
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                EditorGUILayout.Space();
                if (GUILayout.Button($"Setup by {nameof(KanikamaMonitorSetup)}"))
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

        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying && colorsFold;
        }

        void Setup()
        {
            var parent = proxy.transform.parent;
            if (parent == null)
            {
                Debug.LogError($"[Kanikama] {nameof(KanikamaCamera)} and {nameof(KanikamaMonitorSetup)} objects should have a common parent.");
                return;
            }
            var setup = parent.GetComponentInChildren<KanikamaMonitorSetup>();
            if (setup == null)
            {
                Debug.LogError($"[Kanikama] {nameof(KanikamaCamera)} and {nameof(KanikamaMonitorSetup)} objects should have a common parent.");
                return;
            }
            var partitionType = (int)setup.PartitionType;
            var mainMonitor = setup.MainMonitor;
            if (mainMonitor == null)
            {
                Debug.LogError($"[Kanikama] {nameof(KanikamaMonitorSetup)} object has no monitors.");
                return;
            }
            var size = mainMonitor.GetUnrotatedBounds().size;

            partitionTypeProperty.intValue = partitionType;
            aspectRatioProperty.floatValue = size.x / size.y;
            serializedObject.ApplyModifiedProperties();
            UdonSharpEditorUtility.CopyProxyToUdon(proxy);
        }
    }
}