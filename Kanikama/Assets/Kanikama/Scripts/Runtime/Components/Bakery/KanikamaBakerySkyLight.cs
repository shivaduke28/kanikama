#if BAKERY_INCLUDED
using UnityEngine;

namespace Kanikama.Bakery
{
    [RequireComponent(typeof(Light), typeof(BakerySkyLight))]
    public class KanikamaBakerySkyLight : KanikamaLight
    {
        [SerializeField] Light light;
        [SerializeField] BakerySkyLight bakeryLight;

        [SerializeField, HideInInspector] float intensity;
        [SerializeField, HideInInspector] Color color;
        [SerializeField, HideInInspector] bool lightEnabled;

        void OnValidate()
        {
            light = GetComponent<Light>();
            bakeryLight = GetComponent<BakerySkyLight>();
        }
        public override bool Contains(object obj)
        {
            return obj is BakerySkyLight l && l == bakeryLight;
        }

        public override Light GetSource() => light;

        public override void OnBake()
        {
            bakeryLight.enabled = true;
            bakeryLight.color = Color.white;
            bakeryLight.intensity = 1f;
            light.color = Color.white;
            light.intensity = 1f;
            light.enabled = true;
        }

        public override void Rollback()
        {
            bakeryLight.color = color;
            bakeryLight.intensity = intensity;
            bakeryLight.enabled = true;
            light.color = color;
            light.intensity = intensity;
            light.enabled = lightEnabled;
        }

        public override void TurnOff()
        {
            light.enabled = false;
            bakeryLight.enabled = false;
        }

        public override void OnBakeSceneStart()
        {
            intensity = bakeryLight.intensity;
            color = bakeryLight.color;
            lightEnabled = light.enabled;
        }
    }
}
#endif