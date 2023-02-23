using UnityEngine;

namespace Kanikama.GI.Implements
{
    [RequireComponent(typeof(Light))]
    [AddComponentMenu("Kanikama/GI/KanikamaLight")]
    public sealed class KanikamaLight : LightSource
    {
        public override ILightSourceHandle GetHandle()
        {
            return new LightHandle(GetComponent<Light>());
        }
    }
}
