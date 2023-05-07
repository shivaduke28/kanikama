using Kanikama.Core.Attributes;
using UnityEngine;

namespace Kanikama.GI.Baking.Impl
{
    [RequireComponent(typeof(Light))]
    [AddComponentMenu("Kanikama/Baking.KanikamaLight")]
    [EditorOnly]
    public sealed class KanikamaLight : BakeTarget
    {
        [SerializeField] new Light light;
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
