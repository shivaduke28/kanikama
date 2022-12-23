using UnityEditor;

namespace Kanikama.Editor
{
    [CustomEditor(typeof(KanikamaMonitorQuad))]
    class KanikamaMonitorQuadEditor : UnityEditor.Editor
    {
        SerializedProperty isGridRenderersLocked;
        SerializedProperty overridePrefab;
        bool foldAdvanced;

        void OnEnable()
        {
            isGridRenderersLocked = serializedObject.FindProperty(nameof(isGridRenderersLocked));
            overridePrefab = serializedObject.FindProperty(nameof(overridePrefab));
            foldAdvanced = foldAdvanced || isGridRenderersLocked.boolValue;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            foldAdvanced = EditorGUILayout.BeginFoldoutHeaderGroup(foldAdvanced, "Advanced");

            if (foldAdvanced)
            {
                EditorGUILayout.PropertyField(isGridRenderersLocked);
                EditorGUILayout.PropertyField(overridePrefab);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
