using System;
using System.Collections;
using UnityEngine;

namespace Kanikama
{
    [RequireComponent(typeof(Light)), AddComponentMenu("Kanikama/KanikamaUnityLight")]
    public class KanikamaUnityLight : KanikamaLight
    {
        [SerializeField] Light light;
        float intensity;
        Color color;

        private void OnValidate()
        {
            if (light == null)
            {
                light = GetComponent<Light>();
            }
        }

        public override void OnBake()
        {
            light.color = Color.white;
            light.intensity = 1f;
            light.enabled = true;
        }

        public override void Rollback()
        {
            light.intensity = intensity;
            light.color = color;
        }

        public override void TurnOff()
        {
            intensity = light.intensity;
            color = light.color;
            light.enabled = false;
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