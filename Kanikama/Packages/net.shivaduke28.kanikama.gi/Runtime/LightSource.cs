using UnityEngine;

namespace Kanikama.GI
{
    public interface ILightSource
    {
        void Initialize();
        void TurnOff();
        void TurnOn();
        bool Includes(Object obj);
        void Clear();
    }

    public abstract class LightSource : MonoBehaviour, ILightSource
    {
        public abstract void Initialize();
        public abstract void TurnOff();
        public abstract void TurnOn();
        public abstract bool Includes(Object obj);
        public abstract void Clear();
    }
}
