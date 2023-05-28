using UdonSharp;
using UnityEngine;

namespace Kanikama.GI.Udon
{
    public abstract class KanikamaUdonLightSource : UdonSharpBehaviour
    {
        public abstract Color GetLinearColor();
    }
}
