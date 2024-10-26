using UnityEngine;

namespace Kanikama
{
    public interface IBakeTarget
    {
        void Initialize();
        void TurnOff();
        void TurnOn();
        void Clear();
    }

    // todo: 粉砕
    public abstract class BakeTarget : MonoBehaviour, IBakeTarget
    {
        public abstract void Initialize();
        public abstract void TurnOff();
        public abstract void TurnOn();
        public abstract void Clear();
    }
}
