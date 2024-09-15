using UnityEngine;

namespace Kanikama
{
    public interface ILightSourceV2
    {
        // TODO: return IDisposable?
        void Initialize();
        void TurnOff();
        void TurnOn();
        void Clear();
        Color GetLinearColor();
    }

    public abstract class LightSourceV2 : MonoBehaviour, ILightSourceV2
    {
        public abstract void Initialize();
        public abstract void TurnOff();
        public abstract void TurnOn();
        public abstract void Clear();
        public abstract Color GetLinearColor();
    }

    public interface ILightSourceGroupV2
    {
        ILightSourceV2[] GetAll();
        ILightSourceV2 Get(int index);
        Color[] GetLinearColors();
    }

    public abstract class LightSourceGroupV2 : MonoBehaviour, ILightSourceGroupV2
    {
        public abstract ILightSourceV2[] GetAll();
        public abstract ILightSourceV2 Get(int index);
        public abstract Color[] GetLinearColors();
    }
}
