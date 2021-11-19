using System;
using UnityEngine;

namespace Kanikama
{
    public class KanikamaAmbientLight : KanikamaLight
    {
        [SerializeField] new Light light;
        float ambientIntensity;
        Color ambientLight;
        Color ambientSkyColor;
        Color ambientGroundColor;
        Color ambientEquatorColor;

        private void OnValidate()
        {
            if (light == null)
            {
                light = GetComponent<Light>();
            }
        }

        public override Light GetSource() => light;  
        public override bool Contains(object obj)
        {
            return obj is AmbientLightModel;
        }

        public override void OnBake()
        {
            RenderSettings.ambientIntensity = 1;
            RenderSettings.ambientLight = Color.white;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
        }

        public override void Rollback()
        {
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
        }

        public override void TurnOff()
        {
            ambientIntensity = RenderSettings.ambientIntensity;
            ambientLight = RenderSettings.ambientLight;
            ambientSkyColor = RenderSettings.ambientSkyColor;
            ambientEquatorColor = RenderSettings.ambientEquatorColor;
            ambientGroundColor = RenderSettings.ambientGroundColor;

            RenderSettings.ambientIntensity = 0f;
            RenderSettings.ambientLight = Color.black;
            RenderSettings.ambientSkyColor = Color.black;
            RenderSettings.ambientEquatorColor = Color.black;
            RenderSettings.ambientGroundColor = Color.black;
        }
    }
}