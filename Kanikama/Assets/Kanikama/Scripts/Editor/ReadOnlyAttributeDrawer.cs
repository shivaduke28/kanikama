//using Kanikama.EditorOnly;
//using UnityEditor;
//using UnityEngine;

//namespace Kanikama.Editor
//{
//    // https://twitter.com/BinaryImpactG/status/1407261590139936768
//    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
//    public class ReadOnlyAttributeDrawer : PropertyDrawer
//    {
//        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//        {
//            return EditorGUI.GetPropertyHeight(property, label, true);
//        }

//        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//        {
//            var guiState = GUI.enabled;
//            GUI.enabled = false;
//            EditorGUI.PropertyField(position, property, label, true);
//            GUI.enabled = guiState;
//        }
//    }
//}