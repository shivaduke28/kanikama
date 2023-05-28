using UdonSharp;
using UnityEngine;

namespace Kanikama.GI.Udon
{
    public abstract class KanikamaUdonLightSourceGroup : UdonSharpBehaviour
    {
        public abstract Color[] GetLinearColors();
    }
}
