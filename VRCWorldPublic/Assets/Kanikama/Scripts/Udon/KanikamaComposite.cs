using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;

namespace Kanikama.Udon
{
    public class KanikamaComposite : UdonSharpBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private Light[] lights;
        private float[] intensities;
        private Color[] colors;
        private int lightCount;

        private void Start()
        {
            lightCount = lights.Length;
            intensities = new float[lightCount];
            colors = new Color[lightCount];
        }

        private void Update()
        {
            for (var i = 0; i < lightCount; i++)
            {
                var light = lights[i];
                colors[i] = light.color;
                intensities[i] = light.intensity;
            }

            material.SetColorArray("_Colors", colors);
            material.SetFloatArray("_Intensities", intensities);
        }
    }
}