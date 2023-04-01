using Kanikama.GI.Implements;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Editor.GUI
{
    [CustomEditor(typeof(KanikamaMonitorCamera))]
    public class KanikamaMonitorCameraEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var kanikamaMonitorCamera = (KanikamaMonitorCamera) target;
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.Space();
            if (GUILayout.Button("Setup Lights and Camera"))
            {
                Undo.RecordObject(kanikamaMonitorCamera, "Setup Kanikama Monitor Camera");
                kanikamaMonitorCamera.Setup();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
