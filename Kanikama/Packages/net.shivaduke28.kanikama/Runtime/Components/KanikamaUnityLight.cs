using Kanikama.Attributes;
using UnityEngine;

namespace Kanikama.Components
{
    [RequireComponent(typeof(Light))]
    public class KanikamaUnityLight : KanikamaLightSource
    {
        [SerializeField, NonNull] new Light light;
        [SerializeField, HideInInspector] Color color;
        [SerializeField, HideInInspector] float intensity;

        void Reset()
        {
            light = GetComponent<Light>();
        }
#if !COMPILER_UDONSHARP && UNITY_EDITOR

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
#endif

        public override Color GetLinearColor() => light.color.linear * light.intensity;
    }
}
