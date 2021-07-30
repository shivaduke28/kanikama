using UnityEngine;
using UnityEditor;
using Kanikama.EditorOnly;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace Kanikama.Editor
{
    [CustomEditor(typeof(KanikamaMonitorSetup))]
    public class KanikamaMonitorSetupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var monitor = (KanikamaMonitorSetup)target;
            if (monitor.Renderer != null)
            {
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                if (GUILayout.Button("Setup Lights and Camera"))
                {
                    monitor.Setup();
                    var currentScene = SceneManager.GetActiveScene();
                    EditorSceneManager.MarkSceneDirty(currentScene);
                }
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}