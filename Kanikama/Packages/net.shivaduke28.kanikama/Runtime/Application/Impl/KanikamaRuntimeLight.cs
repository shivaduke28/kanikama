using UnityEngine;

namespace Kanikama.Application.Impl
{
    [RequireComponent(typeof(Light))]
    public sealed class KanikamaRuntimeLight : LightSource
    {
        [SerializeField] new Light light;

        void OnValidate()
        {
            light = GetComponent<Light>();
        }

        public override Color GetLinearColor()
        {
            return light.color.linear * light.intensity;
        }
    }
}
