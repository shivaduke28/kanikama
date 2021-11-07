using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor
{
    public static class KanikamaEditorGUI
    {
        public static bool ArrayField(SerializedProperty property, bool fold, bool disable = false)
        {
            fold = EditorGUILayout.BeginFoldoutHeaderGroup(fold, property.displayName);
            if (fold)
            {
                using (new EditorGUI.DisabledGroupScope(disable))
                {
                    var size = property.arraySize;
                    for (var i = 0; i < size; i++)
                    {
                        var prop = property.GetArrayElementAtIndex(i);
                        EditorGUILayout.PropertyField(prop);
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            return fold;
        }
    }
}