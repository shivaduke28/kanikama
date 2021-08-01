using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;

namespace Kanikama.Udon
{
    public class KanikamaCompositor : UdonSharpBehaviour
    {
        [SerializeField] Material[] compositeMaterials;
        [SerializeField] Light[] lights;
        [SerializeField] Renderer[] emissiveRenderers;
        [SerializeField] KanikamaCaptureSampler[] captureSamplers;
        [SerializeField] bool isAmbientEnable;
        [ColorUsage(false, true), SerializeField] Color ambientColor;

        int size;
        Color[] colors;

        int lightCount;

        int rendererCount;
        int[] materialCounts;
        bool[][] materialEmissiveFlags;

        int monitorCount;
        Color[][] monitorColors;
        int[] monitorLightCounts;

        void Start()
        {
            size = 0;

            // A
            if (isAmbientEnable)
            {
                size += 1;
            }

            // L
            lightCount = lights.Length;
            size += lightCount;

            // M
            monitorCount = captureSamplers.Length;
            if (monitorCount > 0)
            {
                monitorColors = new Color[monitorCount][];
                monitorLightCounts = new int[monitorCount];
            }

            for (var i = 0; i < monitorCount; i++)
            {
                var sampler = captureSamplers[i];
                var cols = sampler.GetColors();
                var colCount = cols.Length;
                monitorColors[i] = cols;
                monitorLightCounts[i] = colCount;
                size += colCount;
            }

            // R
            rendererCount = emissiveRenderers.Length;
            if (rendererCount > 0)
            {
                materialCounts = new int[rendererCount];
                materialEmissiveFlags = new bool[rendererCount][];
            }

            for (var i = 0; i < rendererCount; i++)
            {
                var renderer = emissiveRenderers[i];
                var mats = renderer.sharedMaterials;
                var count = mats.Length;
                var flags = new bool[count];
                materialCounts[i] = count;
                for (var j = 0; j < count; j++)
                {
                    var isEmissive = mats[j].IsKeywordEnabled("_EMISSION");
                    flags[j] = isEmissive;
                    if (isEmissive)
                    {
                        size++;
                    }
                }
                materialEmissiveFlags[i] = flags;
            }


            colors = new Color[size];
        }

        void Update()
        {
            var index = 0;

            // A
            if (isAmbientEnable)
            {
                // HDR is linear
                colors[index] = ambientColor.gamma;
                index++;
            }

            // L
            for (var i = 0; i < lightCount; i++)
            {
                var light = lights[i];
                colors[index] = light.color * light.intensity;
                index++;
            }

            // M
            for (var i = 0; i < monitorCount; i++)
            {
                var cols = monitorColors[i];
                var count = monitorLightCounts[i];
                for (var j = 0; j < count; j++)
                {
                    colors[index] = cols[j];
                    index++;
                }
            }

            // R
            for (var i = 0; i < rendererCount; i++)
            {
                var renderer = emissiveRenderers[i];
                var mats = renderer.materials;
                var count = materialCounts[i];
                var emissiveFlags = materialEmissiveFlags[i];
                for (var j = 0; j < count; j++)
                {
                    if (emissiveFlags[j])
                    {
                        colors[index] = mats[j].GetColor("_EmissionColor");
                        index++;
                    }
                }
            }

            foreach (var mat in compositeMaterials)
            {
                mat.SetColorArray("_Colors", colors);
            }
        }
    }
}