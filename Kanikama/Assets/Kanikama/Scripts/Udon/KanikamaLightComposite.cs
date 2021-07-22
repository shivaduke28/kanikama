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
        private float[] intensities;
        private Color[] colors;
        private int lightCount;
        int rendererCount;

        void Start()
        {
            lightCount = lights.Length;
            rendererCount = renderers.Length;
            intensities = new float[lightCount + rendererCount];
            colors = new Color[lightCount + rendererCount];
        }

        void Update()
        {
            for (var i = 0; i < lightCount; i++)
            {
                var light = lights[i];
                colors[i] = light.color;
                intensities[i] = light.intensity;
            }

            for(var i = 0; i < rendererCount; i++)
            {
                var renderer = renderers[i];
                var mat = renderer.sharedMaterial;
                colors[lightCount + i] = mat.GetColor("_EmissionColor");
                intensities[lightCount + i] = 1f;
            }

            foreach (var mat in materials)
            {
                mat.SetColorArray("_Colors", colors);
                mat.SetFloatArray("_Intensities", intensities);
            }
        }
    }
}