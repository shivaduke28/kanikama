using UnityEngine;

namespace Kanikama.GI.Implements
{
    [RequireComponent(typeof(Light))]
    public sealed class KanikamaLight : LightSource
    {
        public override ILightSourceHandle GetHandle()
        {
            return new LightHandle(GetComponent<Light>());
        }
    }
}
