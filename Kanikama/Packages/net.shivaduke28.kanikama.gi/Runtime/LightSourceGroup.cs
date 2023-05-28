using UnityEngine;

namespace Kanikama.GI.Runtime
{
    public interface ILightSourceGroup
    {
        Color[] GetLinearColors();
    }

    public abstract class LightSourceGroup : MonoBehaviour, ILightSourceGroup
    {
        public abstract Color[] GetLinearColors();
    }
}
