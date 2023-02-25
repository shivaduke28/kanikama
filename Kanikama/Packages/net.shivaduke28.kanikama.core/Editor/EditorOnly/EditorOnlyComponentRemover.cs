using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanikama.Core.Editor.EditorOnly
{
    public class EditorOnlyComponentRemover : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (EditorApplication.isPlaying) return;

            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var components = root.GetComponentsInChildren<Component>();
                foreach (var component in components)
                {
                    var editorOnly = component.GetType().GetCustomAttribute<EditorOnlyAttribute>();
                    if (editorOnly != null)
                    {
                        Debug.Log($"[Kanikama] EditorOnly component {component.GetType().Name} is removed from {component.name}");
                        Object.DestroyImmediate(component);
                    }
                }
            }
        }
    }
}