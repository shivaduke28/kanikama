using System;
using Object = UnityEngine.Object;

namespace Kanikama.GI
{
    public interface ILightSourceHandle : IDisposable
    {
        void Initialize();
        void TurnOff();
        void TurnOn();
        bool Includes(Object obj);
    }
}
