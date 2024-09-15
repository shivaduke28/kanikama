using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    public abstract class KanikamaUdonLightSourceGroup : UdonSharpBehaviour
#if !COMPILER_UDONSHARP
        , ILightSourceGroupV2
#endif
    {
#if !COMPILER_UDONSHARP
        public abstract ILightSourceV2[] GetAll();
        public abstract ILightSourceV2 Get(int index);
#endif
        public abstract Color[] GetLinearColors();
    }
}
