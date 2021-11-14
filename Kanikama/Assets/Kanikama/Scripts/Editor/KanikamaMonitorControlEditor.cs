using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor
{
    [CustomEditor(typeof(KanikamaMonitorControl))]
    public class KanikamaMonitorControlEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var monitorControl = (KanikamaMonitorControl)target;
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.Space();
            if (GUILayout.Button("Setup Lights and Camera"))
            {
                monitorControl.Setup();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}