using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Kanikama.Core.Editor
{
    /// <summary>
    /// An attribute to specify a Object type for SerializableGlobalObjectId.
    /// (Serialization for generic type is supported from Unity2020)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SpecifyObjectAttribute : PropertyAttribute
    {
        public Type ObjectType { get; }

        public SpecifyObjectAttribute(Type objectType)
        {
            Assert.IsTrue(objectType.IsSubclassOf(typeof(UnityEngine.Object)));
            ObjectType = objectType;
        }
    }

    [CustomPropertyDrawer(typeof(SpecifyObjectAttribute))]
    public class SpecifyObjectAttributeDrawer : PropertyDrawer
    {
        static readonly Regex ArrayFieldRegex = new Regex(@".Array.data\[([0-9]*)\]$");
        static readonly Regex IndexRegex = new Regex(@"\[([0-9]*)\]$");
        SerializableGlobalObjectIdDrawer drawer; // for single field
        Dictionary<string, SerializableGlobalObjectIdDrawer> drawers; // for array and list
        bool isArray;
        Type objectType;

        bool initialized;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!initialized)
            {
                Initialize(property);
            }

            if (isArray)
            {
                // NOTE: Because each SerializableGlobalObjectIdDrawer may cach a reference of an Object corresponding to GlobalObjectId,
                // we need to create a drawer's instance per array element.
                if (!drawers.TryGetValue(property.propertyPath, out var arrayDrawer))
                {
                    if (!TryParseIndex(property.propertyPath, out var index))
                    {
                        return;
                    }

                    arrayDrawer = new SerializableGlobalObjectIdDrawer(objectType, index);
                    drawers.Add(property.propertyPath, arrayDrawer);
                }

                label.text = $"Element {arrayDrawer.Index}";
                arrayDrawer.Draw(position, property, label);
            }
            else
            {
                drawer.Draw(position, property, label);
            }
        }

        void Initialize(SerializedProperty property)
        {
            var match = ArrayFieldRegex.Match(property.propertyPath);
            isArray = match.Success;

            var attr = (SpecifyObjectAttribute) attribute;
            objectType = attr.ObjectType;

            if (isArray)
            {
                var parent = property.serializedObject;
                var fieldName = property.propertyPath.Substring(0, match.Index);
                var parentProperty = parent.FindProperty(fieldName);
                isArray = parentProperty.isArray;

                drawers = new Dictionary<string, SerializableGlobalObjectIdDrawer>(parentProperty.arraySize);
            }
            else
            {
                drawer = new SerializableGlobalObjectIdDrawer(objectType);
            }
            initialized = true;
        }

        /// <summary>
        /// Try to parse an array index from property.propertyPath.
        /// property.propertyPath is of the form: "{Parent Property Name}.Array.data[{Index}]"
        /// </summary>
        static bool TryParseIndex(string propertyPath, out int index)
        {
            var match = IndexRegex.Match(propertyPath);
            var indexString = propertyPath.Substring(match.Index + 1, match.Length - 2);

            return int.TryParse(indexString, out index);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.type == nameof(SerializableGlobalObjectId))
            {
                return SerializableGlobalObjectIdDrawer.GetPropertyHeight(property, label);
            }

            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
}
