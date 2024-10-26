using Kanikama.Attributes;
using UnityEngine;

namespace Kanikama.Impl
{
    [RequireComponent(typeof(Light))]
    public sealed class KanikamaBakeTargetLight : BakeTarget
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
    }
}
