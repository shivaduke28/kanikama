using Kanikama.Core.Editor.Util;
using Kanikama.GI.Runtime.Impl;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Runtime.Editor
{
    [CustomEditor(typeof(KanikamaRuntimeMonitorCamera))]
    public class KanikamaMonitorCameraEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var kanikamaMonitorCamera = (KanikamaRuntimeMonitorCamera) target;
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
