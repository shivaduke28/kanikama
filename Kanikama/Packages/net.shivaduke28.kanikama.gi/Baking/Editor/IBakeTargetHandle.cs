using Kanikama.Core.Editor;
using Kanikama.GI.Baking;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Editor
{
    public interface IBakeTargetHandle
    {
        string Id { get; }
        void Initialize(string sceneGuid);
        void TurnOff();
        void TurnOn();
        bool Includes(Object obj);
        void Clear();
    }

    public sealed class BakeTargetHandle<T> : IBakeTargetHandle where T : Object, IBakeTarget
    {
        readonly SceneObjectId sceneObjectId;
        ObjectHandle<T> handle;

        string IBakeTargetHandle.Id => sceneObjectId.ToString();

        public BakeTargetHandle(T value)
        {
            var globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(value);
            sceneObjectId = new SceneObjectId(globalObjectId);
        }

        void IBakeTargetHandle.Initialize(string sceneGuid)
        {
            if (ObjectUtility.TryCreateGlobalObjectId(sceneGuid, 2, sceneObjectId.TargetObjectId, sceneObjectId.TargetPrefabId, out var globalObjectId))
            {
                handle = new ObjectHandle<T>(globalObjectId);
            }
            handle.Value.Initialize();
        }

        void IBakeTargetHandle.TurnOff() => handle.Value.TurnOff();
        void IBakeTargetHandle.TurnOn() => handle.Value.TurnOn();
        bool IBakeTargetHandle.Includes(Object obj) => handle.Value.Includes(obj);
        void IBakeTargetHandle.Clear() => handle.Value.Clear();
    }

    public sealed class BakeTargetGroupElementHandle<T> : IBakeTargetHandle where T : Object, IBakeTargetGroup
    {
        readonly SceneObjectId sceneObjectId;
        ObjectHandle<T> handle;
        readonly int index;
        IBakeTarget GetBakeTarget() => handle.Value.Get(index);

        public BakeTargetGroupElementHandle(T value, int index)
        {
            var globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(value);
            sceneObjectId = new SceneObjectId(globalObjectId);
            this.index = index;
        }

        string IBakeTargetHandle.Id => $"{sceneObjectId.ToString()}_{index}";

        void IBakeTargetHandle.Initialize(string sceneGuid)
        {
            if (ObjectUtility.TryCreateGlobalObjectId(sceneGuid, 2, sceneObjectId.TargetObjectId, sceneObjectId.TargetPrefabId, out var globalObjectId))
            {
                handle = new ObjectHandle<T>(globalObjectId);
            }
            GetBakeTarget().Initialize();
        }

        void IBakeTargetHandle.TurnOff() => GetBakeTarget().TurnOff();
        void IBakeTargetHandle.TurnOn() => GetBakeTarget().TurnOn();
        bool IBakeTargetHandle.Includes(Object obj) => GetBakeTarget().Includes(obj);
        void IBakeTargetHandle.Clear() => GetBakeTarget().Clear();
    }
}
