using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor
{
    [CustomEditor(typeof(KanikamaUnityRenderer))]
    public class KanikamaUnityRendererEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var unityRenderer = (KanikamaUnityRenderer)target;
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.Space();
            if (GUILayout.Button("Setup Materials"))
            {
                Undo.RecordObject(unityRenderer, "Setup");
                unityRenderer.Setup();
            }
            EditorGUI.EndDisabledGroup();
        }

    }
}
