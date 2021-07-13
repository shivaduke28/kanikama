using UnityEngine;

namespace Kanikama.Editor
{
    public class KanikamaLightData
    {
        float intensity;
        Color color;
        Light light;
        public bool Enabled { get => light.enabled; set => light.enabled = value; }

        public KanikamaLightData(Light light)
        {
            intensity = light.intensity;
            color = light.color;
            this.light = light;
        }

        public void BeDefault()
        {
            intensity = 1f;
            color = Color.white;
        }

        public void Rollback()
        {
            light.intensity = intensity;
            light.color = color;
        }
    }
}