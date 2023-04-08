using Kanikama.Core.Editor;
using Kanikama.GI.Baking;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Editor
{
    public interface IBakeTargetHandle
    {
        string Id { get; }
        void ReplaceSceneGuid(string sceneGuid);
        void Initialize();
        void TurnOff();
        void TurnOn();
        bool Includes(Object obj);
        void Clear();
    }

    public sealed class BakeTargetHandle<T> : IBakeTargetHandle where T : Object, IBakeTarget
    {
        GlobalObjectId id;
        ObjectHandle<T> handle;

        public override string ToString() => id.ToString();
        string IBakeTargetHandle.Id => id.targetObjectId.ToString();

        public BakeTargetHandle(T value)
        {
            id = GlobalObjectId.GetGlobalObjectIdSlow(value);
            handle = new ObjectHandle<T>(id);
        }

        void IBakeTargetHandle.ReplaceSceneGuid(string sceneGuid)
        {
            if (ObjectUtility.TryCreateGlobalObjectId(sceneGuid, id.identifierType, id.targetObjectId, id.targetPrefabId, out var newId))
            {
                id = newId;
                handle = new ObjectHandle<T>(id);
            }
        }

        void IBakeTargetHandle.Initialize() => handle.Value.Initialize();
        void IBakeTargetHandle.TurnOff() => handle.Value.TurnOff();
        void IBakeTargetHandle.TurnOn() => handle.Value.TurnOn();
        bool IBakeTargetHandle.Includes(Object obj) => handle.Value.Includes(obj);
        void IBakeTargetHandle.Clear() => handle.Value.Clear();
    }

    public sealed class BakeTargetGroupElementHandle<T> : IBakeTargetHandle where T : Object, IBakeTargetGroup
    {
        GlobalObjectId id;
        ObjectHandle<T> handle;
        readonly int index;
        IBakeTarget GetBakeTarget() => handle.Value.Get(index);

        public BakeTargetGroupElementHandle(T value, int index)
        {
            id = GlobalObjectId.GetGlobalObjectIdSlow(value);
            handle = new ObjectHandle<T>(id);
            this.index = index;
        }

        string IBakeTargetHandle.Id => $"{id.targetObjectId}_{index}";

        void IBakeTargetHandle.ReplaceSceneGuid(string sceneGuid)
        {
            if (ObjectUtility.TryCreateGlobalObjectId(sceneGuid, id.identifierType, id.targetObjectId, id.targetPrefabId, out var newId))
            {
                id = newId;
                handle = new ObjectHandle<T>(id);
            }
        }

        void IBakeTargetHandle.Initialize() => GetBakeTarget().Initialize();
        void IBakeTargetHandle.TurnOff() => GetBakeTarget().TurnOff();
        void IBakeTargetHandle.TurnOn() => GetBakeTarget().TurnOn();
        bool IBakeTargetHandle.Includes(Object obj) => GetBakeTarget().Includes(obj);
        void IBakeTargetHandle.Clear() => GetBakeTarget().Clear();
    }
}
