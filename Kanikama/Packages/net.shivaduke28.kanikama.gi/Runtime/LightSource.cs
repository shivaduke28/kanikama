using UnityEngine;

namespace Kanikama.GI
{
    public interface ILightSource
    {
        Color GetColorLinear();
    }

    public abstract class LightSource : Bakeable, ILightSource
    {
        public abstract Color GetColorLinear();
    }
}
