using Kanikama.Baking.Attributes;
using UnityEngine;

namespace Kanikama.Baking.Impl
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
            light.intensity = 0f;
        }

        public override void TurnOn()
        {
            light.color = Color.white;
            light.intensity = 1f;
        }

        public override bool Includes(Object obj) => light == obj;

        public override void Clear()
        {
            light.color = color;
            light.intensity = intensity;
        }
    }
}
