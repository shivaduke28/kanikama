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
        private float[] intensities;
        private Color[] colors;
        private int lightCount;

        void Start()
        {
            lightCount = lights.Length;
            intensities = new float[lightCount];
            colors = new Color[lightCount];
        }

        void Update()
        {
            for (var i = 0; i < lightCount; i++)
            {
                var light = lights[i];
                colors[i] = light.color;
                intensities[i] = light.intensity;
            }

            foreach (var mat in materials)
            {
                mat.SetColorArray("_Colors", colors);
                mat.SetFloatArray("_Intensities", intensities);
            }
        }
    }
}