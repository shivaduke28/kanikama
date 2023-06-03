using System;
using UnityEditor;

namespace Kanikama.Utility.Editor
{
    public readonly struct SceneObjectId : IEquatable<SceneObjectId>
    {
        public ulong TargetObjectId { get; }
        public ulong TargetPrefabId { get; }

        public SceneObjectId(GlobalObjectId globalObjectId)
        {
            if (globalObjectId.identifierType != 2)
            {
                throw new ArgumentException($"GlobalObjectId's identifier type must be 2, but is {globalObjectId.identifierType}");
            }
            TargetObjectId = globalObjectId.targetObjectId;
            TargetPrefabId = globalObjectId.targetPrefabId;
        }

        public SceneObjectId(ulong targetObjectId, ulong targetPrefabId)
        {
            TargetObjectId = targetObjectId;
            TargetPrefabId = targetPrefabId;
        }

        public bool Equals(SceneObjectId other)
        {
            return TargetObjectId == other.TargetObjectId && TargetPrefabId == other.TargetPrefabId;
        }

        public override bool Equals(object obj)
        {
            return obj is SceneObjectId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TargetObjectId.GetHashCode() * 397) ^ TargetPrefabId.GetHashCode();
            }
        }

        public override string ToString() => $"{TargetObjectId}-{TargetPrefabId}";
    }
}
