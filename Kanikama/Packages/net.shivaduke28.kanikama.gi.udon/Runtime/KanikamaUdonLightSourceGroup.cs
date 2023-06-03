using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    public abstract class KanikamaUdonLightSourceGroup : UdonSharpBehaviour
    {
        public abstract Color[] GetLinearColors();
    }
}
