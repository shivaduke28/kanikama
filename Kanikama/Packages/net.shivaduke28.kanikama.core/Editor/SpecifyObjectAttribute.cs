using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Kanikama.Core.Editor
{
    /// <summary>
    /// Because serialization of generic type is supported from Unity2020,
    /// we need to use SpecifyObjectAttribute to specify Object type.
    /// If we want to use SpecifyObjectAttribute for a class other than SerializableGlobalObjectId,
    /// see https://light11.hatenadiary.com/entry/2019/03/24/012712
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SpecifyObjectAttribute : PropertyAttribute
    {
        public Type Type { get; }

        public SpecifyObjectAttribute(Type type)
        {
            Assert.IsTrue(type.IsSubclassOf(typeof(UnityEngine.Object)));
            Type = type;
        }
    }

    [CustomPropertyDrawer(typeof(SpecifyObjectAttribute))]
    public class SpecifyObjectAttributeDrawer : PropertyDrawer
    {
        readonly Dictionary<string, SerializableGlobalObjectIdDrawer> drawers = new Dictionary<string, SerializableGlobalObjectIdDrawer>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // NOTE: When SpecifiedObjectAttribute is used for Array or List,
            // property.propertyPath is of the form: <Parent Property Name>.Array.data[<Index>]
            // Because each SerializableGlobalObjectIdDrawer caches a reference of Object corresponding to GlobalObjectId,
            // we need to create a drawer's instance for each array element property.
            if (!drawers.TryGetValue(property.propertyPath, out var drawer))
            {
                var specifyObjectAttribute = (SpecifyObjectAttribute) attribute;
                drawer = new SerializableGlobalObjectIdDrawer(specifyObjectAttribute.Type);
                drawers.Add(property.propertyPath, drawer);
            }

            drawer.Draw(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return SerializableGlobalObjectIdDrawer.GetPropertyHeight(property, label);
        }
    }
}
