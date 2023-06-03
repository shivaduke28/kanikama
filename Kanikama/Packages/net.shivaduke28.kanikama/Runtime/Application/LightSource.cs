using UnityEngine;

namespace Kanikama.Application
{
    public interface ILightSource
    {
        Color GetLinearColor();
    }

    public abstract class LightSource : MonoBehaviour, ILightSource
    {
        public abstract Color GetLinearColor();
    }
}
