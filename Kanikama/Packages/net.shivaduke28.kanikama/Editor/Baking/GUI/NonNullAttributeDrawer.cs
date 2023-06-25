using Kanikama.Baking.Attributes;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.Baking.GUI
{
    [CustomPropertyDrawer(typeof(NonNullAttribute))]
    internal sealed class NonNullAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var isNull = IsNull(property);
            var color = UnityEngine.GUI.backgroundColor;
            if (isNull)
            {
                UnityEngine.GUI.backgroundColor = Color.red;
            }
            position.height = GetPropertyHeight(property, label);
            EditorGUI.PropertyField(position, property, label, true);
            position.y += position.height;
            if (isNull)
            {
                UnityEngine.GUI.backgroundColor = color;
            }
        }

        static bool IsNull(SerializedProperty property)
        {
            if (property.isArray)
            {
                var length = property.arraySize;
                for (var i = 0; i < length; i++)
                {
                    if (property.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    {
                        return true;
                    }
                }
            }

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                return property.objectReferenceValue == null;
            }

            return false;
        }
    }
}
