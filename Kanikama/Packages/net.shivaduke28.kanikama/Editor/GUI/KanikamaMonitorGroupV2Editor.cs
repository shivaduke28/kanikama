using Kanikama.Components;
using UnityEditor;

namespace Kanikama.Editor.GUI
{
    [CustomEditor(typeof(KanikamaMonitorGroup))]
    public class KanikamaMonitorGroupV2Editor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var kanikamaMonitorGroup = (KanikamaMonitorGroup) target;
            EditorGUI.BeginDisabledGroup(UnityEngine.Application.isPlaying);
            EditorGUILayout.Space();
            if (KanikamaGUI.Button("Setup Camera"))
            {
                Undo.RecordObject(kanikamaMonitorGroup, "Setup Kanikama Monitor Camera");
                kanikamaMonitorGroup.SetupCamera();
            }
            if (KanikamaGUI.Button("Setup Grid"))
            {
                Undo.RecordObject(kanikamaMonitorGroup, "Setup Kanikama Monitor Grid");
                kanikamaMonitorGroup.SetupGridFibers();
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}
