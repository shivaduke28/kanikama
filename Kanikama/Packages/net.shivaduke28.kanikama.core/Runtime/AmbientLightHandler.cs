using UnityEngine;

namespace Kanikama.Core
{
    public sealed class AmbientLightHandler
    {
        float ambientIntensity;
        Color ambientLight;
        Color ambientSkyColor;
        Color ambientGroundColor;
        Color ambientEquatorColor;

        public float Intensity
        {
            get => RenderSettings.ambientIntensity;
            set => RenderSettings.ambientIntensity = value;
        }

        public Color Light
        {
            get => RenderSettings.ambientLight;
            set => RenderSettings.ambientLight = value;
        }

        public Color SkyColor
        {
            get => RenderSettings.ambientSkyColor;
            set => RenderSettings.ambientSkyColor = value;
        }

        public Color EquatorColor
        {
            get => RenderSettings.ambientEquatorColor;
            set => RenderSettings.ambientEquatorColor = value;
        }

        public Color AmbientGroundColor
        {
            get => RenderSettings.ambientGroundColor;
            set => RenderSettings.ambientGroundColor = value;
        }

        public AmbientLightHandler()
        {
        }

        // depends on the current active scene.
        public void Load()
        {
            ambientIntensity = RenderSettings.ambientIntensity;
            ambientLight = RenderSettings.ambientLight;
            ambientSkyColor = RenderSettings.ambientSkyColor;
            ambientEquatorColor = RenderSettings.ambientEquatorColor;
            ambientGroundColor = RenderSettings.ambientGroundColor;
        }

        public void TurnOff()
        {
            RenderSettings.ambientIntensity = 0f;
            RenderSettings.ambientLight = Color.black;
            RenderSettings.ambientSkyColor = Color.black;
            RenderSettings.ambientEquatorColor = Color.black;
            RenderSettings.ambientGroundColor = Color.black;
        }

        public void Revert()
        {
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
        }
    }
}
