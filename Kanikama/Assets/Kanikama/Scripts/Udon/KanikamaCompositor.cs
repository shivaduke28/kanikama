using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;

namespace Kanikama.Udon
{
    public class KanikamaCompositor : UdonSharpBehaviour
    {
        [SerializeField] private Material[] materials;
        [SerializeField] private Light[] lights;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private KanikamaCaptureSampler monitorComposite;
        [SerializeField] private bool isAmbientEnable;
        [SerializeField] [Range(0, 8)] private float ambientIntensity;
        private Color[] colors;
        private int lightCount;
        private int texCount;
        private int rendererCount;
        private int monitorLightCount;
        private Color[] monitorColors;

        private void Start()
        {
            lightCount = lights.Length;
            rendererCount = renderers.Length;
            texCount = lightCount + rendererCount;
            if (monitorComposite != null)
            {
                monitorColors = monitorComposite.GetColors();
                monitorLightCount = monitorColors.Length;
                texCount += monitorLightCount;
            }
            if (isAmbientEnable)
            {
                texCount += 1;
            }
            colors = new Color[texCount];
        }

        private void Update()
        {
            for (var i = 0; i < lightCount; i++)
            {
                var light = lights[i];
                colors[i] = light.color * light.intensity;
            }

            for (var i = 0; i < rendererCount; i++)
            {
                var renderer = renderers[i];
                var mat = renderer.material;
                colors[lightCount + i] = mat.GetColor("_EmissionColor");
            }


            for (var i = 0; i < monitorLightCount; i++)
            {
                var col = monitorColors[i];
                colors[lightCount + rendererCount + i] = col;
            }

            if (isAmbientEnable)
            {
                colors[texCount - 1] = Color.white * ambientIntensity;
            }

            foreach (var mat in materials)
            {
                mat.SetColorArray("_Colors", colors);
            }
        }
    }
}