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
        [SerializeField] private Texture2D tex;

        private float[] intensities;
        private Color[] colors;
        public float intensity = 1f;

        void Start()
        {
            intensities = new float[lightCount];
            colors = new Color[lightCount];
        }


        void OnRenderImage(RenderTexture source, RenderTexture destination)
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