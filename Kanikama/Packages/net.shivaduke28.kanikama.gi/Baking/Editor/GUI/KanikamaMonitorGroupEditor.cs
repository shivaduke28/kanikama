using Kanikama.GI.Baking.Impl;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Editor.GUI
{
    [CustomEditor(typeof(KanikamaMonitorGroup))]
    public class KanikamaMonitorGroupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var kanikamaMonitorGroup = (KanikamaMonitorGroup) target;
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.Space();
            if (GUILayout.Button("Setup Lights"))
            {
                Undo.RecordObject(kanikamaMonitorGroup, "Setup Kanikama Monitor Group");
                kanikamaMonitorGroup.Setup();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
