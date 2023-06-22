using Kanikama.Baking.Attributes;
using UnityEngine;

namespace Kanikama.Udon
{
    [RequireComponent(typeof(Light))]
    public class KanikamaUdonLight : KanikamaUdonLightSource
    {
        [SerializeField, NonNull] new Light light;

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
