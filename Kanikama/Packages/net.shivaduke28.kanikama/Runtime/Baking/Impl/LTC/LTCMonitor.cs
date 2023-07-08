using UnityEngine;

namespace Kanikama.Baking.Impl.LTC
{
    public abstract class LTCMonitor : MonoBehaviour, IBakeTarget
    {
        public abstract void Initialize();
        public abstract void TurnOn();
        public abstract void TurnOff();
        public abstract bool Includes(Object obj);
        public abstract void Clear();
    }
}
