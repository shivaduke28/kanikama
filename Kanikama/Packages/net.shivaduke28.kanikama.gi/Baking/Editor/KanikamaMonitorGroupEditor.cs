using Kanikama.Core.Editor.Util;
using Kanikama.GI.Baking.Impl;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Baking.Editor
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
            if (KanikamaGUI.Button("Setup Lights"))
            {
                Undo.RecordObject(kanikamaMonitorGroup, "Setup Kanikama Monitor Group");
                kanikamaMonitorGroup.Setup();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
