using UnityEngine;

namespace Kanikama.Baking.Experimental.LTC
{
    public abstract class LTCMonitor : MonoBehaviour
    {
        public abstract void Initialize();
        public abstract void TurnOn();
        public abstract void TurnOff();
        public abstract void SetCastShadow(bool enable);
    }
}
