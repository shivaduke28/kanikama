using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.Baking.Util
{
    public static class KanikamaGUI
    {
        public static bool Button(string content) => Button(new GUIContent(content));

        public static bool Button(GUIContent content) => UnityEngine.GUI.Button(
            EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()),
            content);
    }
}
