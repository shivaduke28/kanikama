using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.Bakery
{
    [RequireComponent(typeof(Light), typeof(BakeryPointLight))]
    public class KanikamaBakeryPointLight : KanikamaLight
    {
        [SerializeField] Light light;
        [SerializeField] BakeryPointLight bakeryLight;
        void OnValidate()
        {
            light = GetComponent<Light>();
            bakeryLight = GetComponent<BakeryPointLight>();
        }
        public override bool Contains(object obj)
        {
            return (obj is Light l) && l == light;
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
        }

        public override void TurnOff()
        {
            light.enabled = false;
            bakeryLight.enabled = false;
        }
    }
}