using UnityEngine;

namespace Kanikama.Application
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
