using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    public abstract class KanikamaUdonLightSource : UdonSharpBehaviour
    {
        public abstract Color GetLinearColor();
    }
}
