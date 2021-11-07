using Kanikama.EditorOnly;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Udon.Editor
{
    [CustomEditor(typeof(KanikamaCamera))]
    public class KanikamaCameraEditor : UnityEditor.Editor
    {
        KanikamaCamera proxy;
        SerializedProperty aspectRatioProperty;
        SerializedProperty partitionTypeProperty;

        void OnEnable()
        {
            if (target == null) return;
            proxy = (KanikamaCamera)target;
            aspectRatioProperty = serializedObject.FindProperty("aspectRatio");
            partitionTypeProperty = serializedObject.FindProperty("partitionType");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpEditorUtility.IsProxyBehaviour(proxy))
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                EditorGUILayout.Space();
                if (GUILayout.Button("Apply Setup"))
                {
                    ApplySetup();
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

        void ApplySetup()
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
            var size = mainMonitor.Bounds.size;

            partitionTypeProperty.intValue = partitionType;
            aspectRatioProperty.floatValue = size.x / size.y;
            serializedObject.ApplyModifiedProperties();
            UdonSharpEditorUtility.CopyProxyToUdon(proxy);
        }
    }
}