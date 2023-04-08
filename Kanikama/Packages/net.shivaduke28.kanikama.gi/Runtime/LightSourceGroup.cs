using UnityEngine;

namespace Kanikama.GI.Runtime
{
    public interface ILightSourceGroup
    {
        Color[] GetColors();
    }

    public abstract class LightSourceGroup : MonoBehaviour, ILightSourceGroup
    {
        public abstract Color[] GetColors();
    }
}
