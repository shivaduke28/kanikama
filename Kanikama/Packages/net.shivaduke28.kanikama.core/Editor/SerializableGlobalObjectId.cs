using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.Core.Editor
{
    [Serializable]
    public sealed class SerializableGlobalObjectId : IEquatable<SerializableGlobalObjectId>
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
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return assetGUID == other.assetGUID && identifierType == other.identifierType && targetObjectId == other.targetObjectId && targetPrefabId == other.targetPrefabId;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is SerializableGlobalObjectId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (assetGUID != null ? assetGUID.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ identifierType;
                hashCode = (hashCode * 397) ^ (targetObjectId != null ? targetObjectId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (targetPrefabId != null ? targetPrefabId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public sealed class SerializableGlobalObjectIdDrawer
    {
        Object obj;
        readonly Type type;
        public int Index { get; }
        bool isChanged = true;
        static readonly GUIContent BlankLabel = new GUIContent(" ");
        GlobalObjectId lastId;

        public SerializableGlobalObjectIdDrawer(Type type)
        {
            this.type = type;
        }

        public SerializableGlobalObjectIdDrawer(Type type, int index)
        {
            this.type = type;
            Index = index;
        }

        public void Draw(Rect position, SerializedProperty property, GUIContent label)
        {
            if (isChanged)
            {
                if (TryParse(property, out var currentId))
                {
                    if (!currentId.Equals(lastId))
                    {
                        lastId = currentId;
                        obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(currentId);
                    }
                }
                else
                {
                    lastId = default;
                    obj = null;
                }
                isChanged = false;
            }

            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                obj = EditorGUI.ObjectField(rect, BlankLabel, obj, type, true);
                if (check.changed)
                {
                    lastId = GlobalObjectId.GetGlobalObjectIdSlow(obj);
                    Set(property, lastId);
                }
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.PropertyField(position, property, label, true);
                if (check.changed)
                {
                    isChanged = true;
                }
            }
        }

        public static float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
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
