using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;

namespace Kanikama.Udon
{
    public class KanikamaLightComposite : UdonSharpBehaviour
    {
        [SerializeField] private Material[] materials;
        [SerializeField] private Light[] lights;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private bool isAmbientEnable;
        [SerializeField] [Range(0, 8)] private float ambientIntensity;
        private Color[] colors;
        private int lightCount;
        private int texCount;
        int rendererCount;

        void Start()
        {
            lightCount = lights.Length;
            rendererCount = renderers.Length;
            texCount = lightCount + rendererCount;
            if (isAmbientEnable)
            {
                texCount += 1;
            }
            colors = new Color[texCount];
        }

        void Update()
        {
            for (var i = 0; i < lightCount; i++)
            {
                var light = lights[i];
                colors[i] = light.color * light.intensity;
            }

            for(var i = 0; i < rendererCount; i++)
            {
                var renderer = renderers[i];
                var mat = renderer.sharedMaterial;
                colors[lightCount + i] = mat.GetColor("_EmissionColor");
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