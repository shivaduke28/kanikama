using Kanikama.Baking.Impl;
using Kanikama.Editor.Baking;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.Baking.GUI
{
    [CustomEditor(typeof(KanikamaBakeTargetMonitorGroup))]
    public class KanikamaMonitorGroupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var kanikamaMonitorGroup = (KanikamaBakeTargetMonitorGroup) target;
            EditorGUI.BeginDisabledGroup(UnityEngine.Application.isPlaying);
            EditorGUILayout.Space();
            if (KanikamaGUI.Button("Setup Lights"))
            {
                Undo.RecordObject(kanikamaMonitorGroup, "Setup Kanikama Monitor Group");
                kanikamaMonitorGroup.Setup();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
