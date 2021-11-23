using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor
{
    [CustomEditor(typeof(KanikamaMonitorController))]
    public class KanikamaMonitorControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var monitorControl = (KanikamaMonitorController)target;
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.Space();
            if (GUILayout.Button("Setup Lights and Camera"))
            {
                Undo.RecordObject(monitorControl, "Setup Monitor Control");
                monitorControl.Setup();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}