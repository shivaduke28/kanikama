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

        public void OnPreBake()
        {
            intensity = light.intensity;
            color = light.color;
            light.color = Color.white;
            light.intensity = 1f;
            light.enabled = false;
        }

        public void OnPostBake()
        {
            light.intensity = intensity;
            light.color = color;
            light.enabled = true;
        }
    }
}