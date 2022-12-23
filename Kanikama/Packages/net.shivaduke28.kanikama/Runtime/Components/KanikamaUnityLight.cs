using UnityEngine;

namespace Kanikama
{
    [RequireComponent(typeof(Light))]
    public class KanikamaUnityLight : KanikamaLight
    {
        [SerializeField] Light light;
        [SerializeField, HideInInspector] float intensity;
        [SerializeField, HideInInspector] Color color;

        private void OnValidate()
        {
            if (light == null)
            {
                light = GetComponent<Light>();
            }
        }
        public override void OnBakeSceneStart()
        {
            intensity = light.intensity;
            color = light.color;
        }

        public override void OnBake()
        {
            light.color = Color.white;
            light.intensity = 1f;
        }

        public override void Rollback()
        {
            light.intensity = intensity;
            light.color = color;
        }

        public override void TurnOff()
        {
            light.intensity = 0;
        }

        public override bool Contains(object obj)
        {
            if (obj is Light l)
            {
                return l == light;
            }
            return false;
        }

        public override Light GetSource()
        {
            return light;
        }
    }
}