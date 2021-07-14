using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;

namespace Kanikama.Udon
{
    public class KanikamaMonitorComposite : UdonSharpBehaviour
    {
        [SerializeField] private Material[] materials;
        [SerializeField] private int lightCount;

        private float[] intensities;
        private Color[] colors;


        [SerializeField] private Texture2D tex;

        void Start()
        {
            intensities = new float[lightCount];
            colors = new Color[lightCount];
        }

        void Update()
        {
            //var pix = texture.ReadPixels();
            //for (var i = 0; i < lightCount; i++)
            //{
            //    var light = lights[i];
            //    colors[i] = light.color;
            //    intensities[i] = light.intensity;
            //}

            //foreach (var mat in materials)
            //{
            //    mat.SetColorArray("_Colors", colors);
            //    mat.SetFloatArray("_Intensities", intensities);
            //}
        }

        public float intensity = 1f;

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            tex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            tex.Apply();
            colors = tex.GetPixels(6);
            for (var i = 0; i < lightCount; i++)
            {
                intensities[i] = intensity;
            }

            foreach (var mat in materials)
            {
                mat.SetColorArray("_Colors", colors);
                mat.SetFloatArray("_Intensities", intensities);
            }
        }
    }
}