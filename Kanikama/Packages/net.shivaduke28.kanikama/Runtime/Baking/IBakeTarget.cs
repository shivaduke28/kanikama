using UnityEngine;

namespace Kanikama.Baking
{
    public interface IBakeTarget
    {
        void Initialize();
        void TurnOff();
        void TurnOn();
        void Clear();
    }

    public abstract class BakeTarget : MonoBehaviour, IBakeTarget
    {
        public abstract void Initialize();
        public abstract void TurnOff();
        public abstract void TurnOn();
        public abstract void Clear();
    }
}
