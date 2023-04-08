using UnityEngine;

namespace Kanikama.GI.Runtime
{
    public interface ILightSource
    {
        Color GetColorLinear();
    }

    public abstract class LightSource : MonoBehaviour, ILightSource
    {
        public abstract Color GetColorLinear();
    }
}
