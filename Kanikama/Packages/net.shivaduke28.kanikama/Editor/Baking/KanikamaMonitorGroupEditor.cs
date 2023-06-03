using Kanikama.Baking.Impl;
using Kanikama.Core.Editor.Util;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Baking.Editor
{
    [CustomEditor(typeof(KanikamaBakeTargetMonitorGroup))]
    public class KanikamaMonitorGroupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var kanikamaMonitorGroup = (KanikamaBakeTargetMonitorGroup) target;
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
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
