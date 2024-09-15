using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    public abstract class KanikamaUdonLightSource : UdonSharpBehaviour
#if !COMPILER_UDONSHARP
        , ILightSourceV2
#endif
    {
#if !COMPILER_UDONSHARP
        public abstract void Initialize();
        public abstract void TurnOff();
        public abstract void TurnOn();
        public abstract void Clear();
#endif
        public abstract Color GetLinearColor();
    }
}
