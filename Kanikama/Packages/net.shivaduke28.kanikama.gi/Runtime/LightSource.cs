using UnityEngine;

namespace Kanikama.GI
{
    public abstract class LightSource : MonoBehaviour
    {
        public abstract ILightSourceHandle GetHandle();
    }
}
