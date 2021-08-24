using Kanikama.EditorOnly;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor
{
    [CustomEditor(typeof(KanikamaMonitorSetup))]
    public class KanikamaMonitorSetupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var monitor = (KanikamaMonitorSetup)target;
            if (monitor.Monitors.Count != 0)
            {
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                if (GUILayout.Button("Setup Lights and Camera"))
                {
                    var tagets = monitor.Monitors;
                    Undo.RecordObjects(tagets.ToArray(), "Setup");
                    monitor.Setup();
                }
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}