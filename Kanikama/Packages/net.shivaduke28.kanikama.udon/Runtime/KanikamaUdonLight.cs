using UnityEngine;

namespace Kanikama.Udon
{
    [RequireComponent(typeof(Light))]
    public class KanikamaUdonLight : KanikamaUdonLightSource
    {
        [SerializeField, NonNull] new Light light;
#if !COMPILER_UDONSHARP
        [SerializeField] Color color;
        [SerializeField] float intensity;

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
        void OnValidate()
        {
            if (light == null)
            {
                light = GetComponent<Light>();
            }
        }

        public override Color GetLinearColor()
        {
            if (!gameObject.activeSelf) return Color.black;
            return light.color.linear * light.intensity;
        }
    }
}
