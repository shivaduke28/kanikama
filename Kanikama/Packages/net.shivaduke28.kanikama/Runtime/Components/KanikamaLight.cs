using Kanikama.Attributes;
using UnityEngine;

namespace Kanikama.Components
{
    [RequireComponent(typeof(Light))]
    public sealed class KanikamaLight : LightSourceV2
    {
        [SerializeField, NonNull] new Light light;
        [SerializeField, HideInInspector] Color color;
        [SerializeField, HideInInspector] float intensity;

        void OnValidate()
        {
            light = GetComponent<Light>();
        }

        public override void Initialize()
        {
            color = light.color;
            intensity = light.intensity;
        }

        public override void TurnOff()
        {
            light.intensity = 0;
        }

        public override void TurnOn()
        {
            light.color = Color.white;
            light.intensity = 1f;
        }

        public override void Clear()
        {
            light.color = color;
            light.intensity = intensity;
        }

        public override Color GetLinearColor() => light.color.linear * light.intensity;
    }
}
