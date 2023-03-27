using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.Core.Editor
{
    [Serializable]
    public struct SerializableGlobalObjectId : IEquatable<SerializableGlobalObjectId>
    {
        [SerializeField] public string assetGUID;
        [SerializeField] public int identifierType;
        [SerializeField] public string targetObjectId;
        [SerializeField] public string targetPrefabId;

        public override string ToString()
        {
            return $"GlobalObjectId_V1-{identifierType}-{assetGUID}-{targetObjectId}-{targetPrefabId}";
        }

        public bool TryParse(out GlobalObjectId globalObjectId)
        {
            return GlobalObjectId.TryParse(ToString(), out globalObjectId);
        }

        public static SerializableGlobalObjectId Create(GlobalObjectId globalObjectId)
        {
            return new SerializableGlobalObjectId
            {
                assetGUID = globalObjectId.assetGUID.ToString(),
                identifierType = globalObjectId.identifierType,
                targetObjectId = globalObjectId.targetObjectId.ToString(),
                targetPrefabId = globalObjectId.targetPrefabId.ToString()
            };
        }

        public bool Equals(SerializableGlobalObjectId other)
        {
            return assetGUID.Equals(other.assetGUID) && identifierType == other.identifierType && targetObjectId == other.targetObjectId &&
                targetPrefabId == other.targetPrefabId;
        }

        public override bool Equals(object obj)
        {
            return obj is SerializableGlobalObjectId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = assetGUID.GetHashCode();
                hashCode = (hashCode * 397) ^ identifierType;
                hashCode = (hashCode * 397) ^ targetObjectId.GetHashCode();
                hashCode = (hashCode * 397) ^ targetPrefabId.GetHashCode();
                return hashCode;
            }
        }
    }

    public sealed class SerializableGlobalObjectIdDrawer
    {
        Object obj;
        readonly Type type;
        bool isSearch;

        public SerializableGlobalObjectIdDrawer(Type type)
        {
            this.type = type;
        }

        public void Draw(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!isSearch && obj == null && TryParse(property, out var currentId))
            {
                obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(currentId);
                isSearch = true;
            }

            var rect = new Rect(position);
            rect.height = 17f;
            EditorGUI.BeginChangeCheck();
            obj = EditorGUI.ObjectField(rect, obj, type, true);

            if (EditorGUI.EndChangeCheck())
            {
                var id = GlobalObjectId.GetGlobalObjectIdSlow(obj);
                Set(property, id);
            }

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property, label, true);
        }

        public static float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.singleLineHeight;
        }

        static void Set(SerializedProperty property, GlobalObjectId globalObjectId)
        {
            var assetGUID = property.FindPropertyRelative("assetGUID");
            assetGUID.stringValue = globalObjectId.assetGUID.ToString();
            var identifierType = property.FindPropertyRelative("identifierType");
            identifierType.intValue = globalObjectId.identifierType;
            var targetObjectId = property.FindPropertyRelative("targetObjectId");
            targetObjectId.stringValue = globalObjectId.targetObjectId.ToString();
            var targetPrefabId = property.FindPropertyRelative("targetPrefabId");
            targetPrefabId.stringValue = globalObjectId.targetPrefabId.ToString();
        }

        static bool TryParse(SerializedProperty property, out GlobalObjectId globalObjectId)
        {
            var assetGUID = property.FindPropertyRelative("assetGUID");
            var identifierType = property.FindPropertyRelative("identifierType");
            var targetObjectId = property.FindPropertyRelative("targetObjectId");
            var targetPrefabId = property.FindPropertyRelative("targetPrefabId");
            return GlobalObjectId.TryParse(
                $"GlobalObjectId_V1-{identifierType.intValue}-{assetGUID.stringValue}-{targetObjectId.stringValue}-{targetPrefabId.stringValue}",
                out globalObjectId);
        }
    }
}
