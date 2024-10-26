using UnityEngine;

namespace Kanikama
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
