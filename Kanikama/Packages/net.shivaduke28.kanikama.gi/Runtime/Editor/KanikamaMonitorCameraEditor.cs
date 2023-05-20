using Kanikama.Core.Editor.Util;
using Kanikama.GI.Runtime.Impl;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Runtime.Editor
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
            if (KanikamaGUI.Button("Setup Camera"))
            {
                Undo.RecordObject(kanikamaMonitorCamera, "Setup Kanikama Monitor Camera");
                kanikamaMonitorCamera.Setup();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
