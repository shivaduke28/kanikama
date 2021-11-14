//using UdonSharp;
//using UdonSharpEditor;
//using UnityEditor;
//using UnityEngine;

//namespace Kanikama.Udon.Editor
//{
//    [CustomEditor(typeof(KanikamaRealtimeMonitorLight))]
//    public class KanikamaRealtimeMonitorLightEditor : UnityEditor.Editor
//    {
//        UdonSharpBehaviour proxy;
//        SerializedProperty weightsProperty;
//        SerializedProperty kanikamaCameraProperty;

//        void OnEnable()
//        {
//            if (target == null) return;
//            proxy = (UdonSharpBehaviour)target;
//            weightsProperty = serializedObject.FindProperty("weights");
//            kanikamaCameraProperty = serializedObject.FindProperty("kanikamaCamera");
//        }

//        public override void OnInspectorGUI()
//        {
//            if (UdonSharpEditorUtility.IsProxyBehaviour(proxy))
//            {
//                base.OnInspectorGUI();
//                EditorGUILayout.Space();
//                if (GUILayout.Button($"Reset weights"))
//                {
//                    ResetWeights();
//                }
//            }
//            else
//            {
//                if (UdonSharpGUI.DrawConvertToUdonBehaviourButton(target))
//                {
//                    return;
//                }
//            }
//        }

//        void ResetWeights()
//        {
//            var lightmapCount = 0;
//            var cameraRef = kanikamaCameraProperty.objectReferenceValue;

//            if (cameraRef is KanikamaCamera kanikamaCamera)
//            {
//                var monitorSetup = kanikamaCamera.transform.parent.GetComponentInChildren<KanikamaMonitorSetup>();
//                var partitionType = (int)monitorSetup.PartitionType;
//                lightmapCount = (partitionType % 10) * Mathf.FloorToInt(partitionType / 10f);
//            }

//            weightsProperty.arraySize = lightmapCount;
//            for (var i = 0; i < lightmapCount; i++)
//            {
//                var prop = weightsProperty.GetArrayElementAtIndex(i);
//                prop.floatValue = 1f;
//            }

//            serializedObject.ApplyModifiedProperties();
//            UdonSharpEditorUtility.CopyProxyToUdon(proxy);
//        }
//    }
//}