using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Udon.Editor
{
    [CustomEditor(typeof(KanikamaMonitorCamera))]
    public class KanikamaMonitorCameraEditor : UnityEditor.Editor
    {
        KanikamaMonitorCamera kanikamaMonitorCamera;

        void OnEnable()
        {
            if (target == null) return;
            kanikamaMonitorCamera = (KanikamaMonitorCamera) target;
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
