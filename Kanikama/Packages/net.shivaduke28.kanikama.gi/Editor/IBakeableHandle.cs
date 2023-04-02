using Kanikama.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Editor
{
    public interface IBakeableHandle
    {
        string Id { get; }
        void ReplaceSceneGuid(string sceneGuid);
        void Initialize();
        void TurnOff();
        void TurnOn();
        bool Includes(Object obj);
        void Clear();
    }

    public sealed class BakeableHandle<T> : IBakeableHandle where T : Object, IBakeable
    {
        readonly SerializableGlobalObjectId id;
        T value;
        T Value => value != null ? value : value = FindSlow();

        T FindSlow()
        {
            if (id.TryParse(out var globalObjectId))
            {
                var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
                return (T) obj;
            }
            return default;
        }

        public override string ToString() => id.ToString();
        public string Id => id.targetObjectId;

        public BakeableHandle(T value)
        {
            this.value = value;
            id = SerializableGlobalObjectId.Create(GlobalObjectId.GetGlobalObjectIdSlow(value));
        }

        void IBakeableHandle.ReplaceSceneGuid(string sceneGuid)
        {
            id.assetGUID = sceneGuid;
            value = null;
        }

        void IBakeableHandle.Initialize() => Value.Initialize();
        void IBakeableHandle.TurnOff() => Value.TurnOff();
        void IBakeableHandle.TurnOn() => Value.TurnOn();
        bool IBakeableHandle.Includes(Object obj) => Value.Includes(obj);
        void IBakeableHandle.Clear() => Value.Clear();
    }
}
