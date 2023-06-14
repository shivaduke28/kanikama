using Kanikama.Baking.Experimental.LTC;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Kanikama.Editor.Baking.Experimental.LTC
{
    public interface ILTCMonitorHandle
    {
        string Name { get; }
        string Id { get; }
        void Initialize(string sceneGuid);
        void TurnOff();
        void TurnOn();
        void SetCastShadow(bool enable);
        bool Includes(Object obj);
    }

    public class LTCMonitorHandle<T> : ILTCMonitorHandle where T : LTCMonitor
    {
        readonly SceneObjectId sceneObjectId;
        readonly string name;
        ObjectHandle<T> handle;

        public LTCMonitorHandle(T value)
        {
            var globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(value);
            sceneObjectId = new SceneObjectId(globalObjectId);
            name = value.name;
        }

        public string Name => name;
        public string Id => sceneObjectId.ToString();

        public void Initialize(string sceneGuid)
        {
            if (GlobalObjectIdHelper.TryParse(sceneGuid, 2,
                    sceneObjectId.TargetObjectId, sceneObjectId.TargetPrefabId, out var globalObjectId))
            {
                handle = new ObjectHandle<T>(globalObjectId);
            }
        }

        public void TurnOff() => handle.Value.TurnOff();

        public void TurnOn() => handle.Value.TurnOn();

        public void SetCastShadow(bool enable) => handle.Value.SetCastShadow(enable);

        public bool Includes(Object obj) => handle.Value.Includes(obj);
    }
}
