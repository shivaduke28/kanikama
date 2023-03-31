using Kanikama.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Implements
{
    [RequireComponent(typeof(Light))]
    [AddComponentMenu("Kanikama/GI/KanikamaLight")]
    [EditorOnly]
    public sealed class KanikamaLight : LightSource
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
