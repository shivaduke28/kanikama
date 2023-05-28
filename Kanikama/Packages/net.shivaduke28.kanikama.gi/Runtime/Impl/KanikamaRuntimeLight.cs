using UnityEngine;

namespace Kanikama.GI.Runtime.Impl
{
    [RequireComponent(typeof(Light))]
    public sealed class KanikamaRuntimeLight : LightSource
    {
        [SerializeField] new Light light;

        void OnValidate()
        {
            light = GetComponent<Light>();
        }

        public override Color GetColorLinear()
        {
            return light.color.linear * light.intensity;
        }
    }
}
