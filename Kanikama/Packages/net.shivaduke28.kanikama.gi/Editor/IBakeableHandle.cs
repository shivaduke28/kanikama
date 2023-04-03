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
        GlobalObjectId id;
        ObjectHandle<T> handle;

        public override string ToString() => id.ToString();
        string IBakeableHandle.Id => id.targetObjectId.ToString();

        public BakeableHandle(T value)
        {
            id = GlobalObjectId.GetGlobalObjectIdSlow(value);
            handle = new ObjectHandle<T>(id);
        }

        void IBakeableHandle.ReplaceSceneGuid(string sceneGuid)
        {
            if (ObjectUtility.TryCreateGlobalObjectId(sceneGuid, id.identifierType, id.targetObjectId, id.targetPrefabId, out var newId))
            {
                id = newId;
                handle = new ObjectHandle<T>(id);
            }
        }

        void IBakeableHandle.Initialize() => handle.Value.Initialize();
        void IBakeableHandle.TurnOff() => handle.Value.TurnOff();
        void IBakeableHandle.TurnOn() => handle.Value.TurnOn();
        bool IBakeableHandle.Includes(Object obj) => handle.Value.Includes(obj);
        void IBakeableHandle.Clear() => handle.Value.Clear();
    }

    public sealed class BakeableGroupElementHandle<T> : IBakeableHandle where T : Object, IBakeableGroup
    {
        GlobalObjectId id;
        ObjectHandle<T> handle;
        readonly int index;
        IBakeable GetBakeable() => handle.Value.Get(index);

        public BakeableGroupElementHandle(T value, int index)
        {
            id = GlobalObjectId.GetGlobalObjectIdSlow(value);
            handle = new ObjectHandle<T>(id);
            this.index = index;
        }

        string IBakeableHandle.Id => $"{id.targetObjectId}_{index}";

        void IBakeableHandle.ReplaceSceneGuid(string sceneGuid)
        {
            if (ObjectUtility.TryCreateGlobalObjectId(sceneGuid, id.identifierType, id.targetObjectId, id.targetPrefabId, out var newId))
            {
                id = newId;
                handle = new ObjectHandle<T>(id);
            }
        }

        void IBakeableHandle.Initialize() => GetBakeable().Initialize();
        void IBakeableHandle.TurnOff() => GetBakeable().TurnOff();
        void IBakeableHandle.TurnOn() => GetBakeable().TurnOn();
        bool IBakeableHandle.Includes(Object obj) => GetBakeable().Includes(obj);
        void IBakeableHandle.Clear() => GetBakeable().Clear();
    }
}
