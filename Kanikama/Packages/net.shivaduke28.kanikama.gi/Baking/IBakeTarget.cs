using UnityEngine;

namespace Kanikama.GI.Baking
{
    public interface IBakeTarget
    {
        void Initialize();
        void TurnOff();
        void TurnOn();
        bool Includes(Object obj);
        void Clear();
    }

    public abstract class BakeTarget : MonoBehaviour, IBakeTarget
    {
        public abstract void Initialize();
        public abstract void TurnOff();
        public abstract void TurnOn();
        public abstract bool Includes(Object obj);
        public abstract void Clear();
    }

}
