using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Udon.Editor
{
    [CustomEditor(typeof(KanikamaUdonMonitorCamera))]
    public sealed class KanikamaUdonMonitorCameraEditor : UnityEditor.Editor
    {
        KanikamaUdonMonitorCamera kanikamaMonitorCamera;

        void OnEnable()
        {
            if (target == null) return;
            kanikamaMonitorCamera = (KanikamaUdonMonitorCamera) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("Setup Camera"))
                {
                    Undo.RecordObject(kanikamaMonitorCamera, "Setup Kanikama Monitor Camera");
                    kanikamaMonitorCamera.Setup();
                    UdonSharpEditorUtility.CopyProxyToUdon(kanikamaMonitorCamera);
                }
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }
    }
}
