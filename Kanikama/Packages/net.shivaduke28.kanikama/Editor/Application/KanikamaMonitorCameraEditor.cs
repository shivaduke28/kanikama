using Kanikama.Application.Impl;
using Kanikama.Core.Editor.Util;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Application.Editor
{
    [CustomEditor(typeof(KanikamaRuntimeMonitorCamera))]
    public class KanikamaMonitorCameraEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var kanikamaMonitorCamera = (KanikamaRuntimeMonitorCamera) target;
            EditorGUI.BeginDisabledGroup(UnityEngine.Application.isPlaying);
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
