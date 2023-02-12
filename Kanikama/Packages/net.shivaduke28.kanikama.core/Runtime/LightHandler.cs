using UnityEngine;

namespace Kanikama.Core
{
    public sealed class LightHandler
    {
        readonly Light light;
        readonly Color color;
        readonly float intensity;

        public Color Color
        {
            get => light.color;
            set => light.color = value;
        }

        public float Intensity
        {
            get => light.intensity;
            set => light.intensity = value;
        }

        public LightHandler(Light light)
        {
            this.light = light;
            color = light.color;
            intensity = light.intensity;
        }

        public void TurnOff()
        {
            light.intensity = 0;
        }

        public void Revert()
        {
            light.color = color;
            light.intensity = intensity;
        }
    }
}
