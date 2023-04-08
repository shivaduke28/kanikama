﻿using Kanikama.GI.Baking;
using UnityEngine;

namespace Kanikama.GI.Bakery.Baking
{
    [RequireComponent(typeof(Light), typeof(BakerySkyLight))]
    [AddComponentMenu("Kanikama/GI/Baking.KanikamaBakerySkyLight")]
    public sealed class KanikamaBakerySkyLight : BakeTarget
    {
        [SerializeField] new Light light;
        [SerializeField] BakerySkyLight bakeryLight;

        [SerializeField, HideInInspector] float intensity;
        [SerializeField, HideInInspector] Color color;
        [SerializeField, HideInInspector] bool lightEnabled;

        void OnValidate()
        {
            if (light == null) light = GetComponent<Light>();
            if (bakeryLight == null) bakeryLight = GetComponent<BakerySkyLight>();
        }

        public override void Initialize()
        {
            intensity = bakeryLight.intensity;
            color = bakeryLight.color;
            lightEnabled = light.enabled;
        }

        public override void TurnOff()
        {
            light.enabled = false;
            bakeryLight.enabled = false;
        }

        public override void TurnOn()
        {
            bakeryLight.enabled = true;
            bakeryLight.color = Color.white;
            bakeryLight.intensity = 1f;
            light.color = Color.white;
            light.intensity = 1f;
            light.enabled = true;
        }

        public override bool Includes(Object obj)
        {
            return obj is BakerySkyLight l && l == bakeryLight;
        }

        public override void Clear()
        {
            bakeryLight.color = color;
            bakeryLight.intensity = intensity;
            bakeryLight.enabled = true;
            light.color = color;
            light.intensity = intensity;
            light.enabled = lightEnabled;
        }
    }
}
