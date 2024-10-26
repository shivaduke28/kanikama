using UnityEngine;

namespace Kanikama.Impl.LTC
{
    public abstract class LTCMonitor : MonoBehaviour, IKanikamaBakeTarget
    {
        public abstract void Initialize();
        public abstract void TurnOn();
        public abstract void TurnOff();
        public abstract bool Includes(Object obj);
        public abstract void Clear();
    }
}
