using UnityEditor;
using UnityEngine;

namespace Kanikama.Core.Editor.Util
{
    public static class KanikamaGUI
    {
        public static bool Button(string content) => Button(new GUIContent(content));

        public static bool Button(GUIContent content) => UnityEngine.GUI.Button(
            EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()),
            content);
    }
}
