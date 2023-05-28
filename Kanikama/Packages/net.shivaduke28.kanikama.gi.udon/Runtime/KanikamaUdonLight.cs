using UnityEngine;

namespace Kanikama.GI.Udon
{
    [RequireComponent(typeof(Light))]
    public class KanikamaUdonLight : KanikamaUdonLightSource
    {
        [SerializeField] Light light;

        void OnValidate()
        {
            if (light == null)
            {
                light = GetComponent<Light>();
            }
        }

        public override Color GetLinearColor()
        {
            return light.color.linear * light.intensity;
        }
    }
}
