using UnityEngine;

namespace Kanikama.Baking.Impl.LTC
{
    public abstract class KanikamaLTCMonitor : MonoBehaviour
    {
        public abstract void Initialize();
        public abstract void TurnOn();
        public abstract void TurnOff();
    }
}
