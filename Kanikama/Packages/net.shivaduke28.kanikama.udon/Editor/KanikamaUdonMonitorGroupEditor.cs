using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Udon.Editor
{
    [CustomEditor(typeof(KanikamaUdonMonitorGroup))]
    public sealed class KanikamaUdonMonitorGroupEditor : UnityEditor.Editor
    {
        KanikamaUdonMonitorGroup kanikamaMonitorGroup;

        void OnEnable()
        {
            if (target == null) return;
            kanikamaMonitorGroup = (KanikamaUdonMonitorGroup) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(UnityEngine.Application.isPlaying))
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("Setup Camera"))
                {
                    Undo.RecordObject(kanikamaMonitorGroup, "Setup Kanikama Monitor Camera");
                    kanikamaMonitorGroup.SetupCamera();
                    UdonSharpEditorUtility.CopyProxyToUdon(kanikamaMonitorGroup);
                }
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return UnityEngine.Application.isPlaying;
        }
    }
}
