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
        [SerializeField] [Range(0, 8)] float ambientIntensity;
        Color[] colors;
        int lightCount;
        int texCount;
        int rendererCount;
        Color[][] monitorColors2;
        bool[][] materialEmissiveFlags;
        int[] materialCounts;
        int[] monitorLightCounts;
        int monitorCount;

        void Start()
        {
            lightCount = lights.Length;

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
                        texCount++;
                    }
                }
                materialEmissiveFlags[i] = flags;
            }

            monitorCount = captureSamplers.Length;
            if (monitorCount > 0)
            {
                monitorColors2 = new Color[monitorCount][];
                monitorLightCounts = new int[monitorCount];
            }

            for (var i = 0; i < monitorCount; i++)
            {
                var sampler = captureSamplers[i];
                var cols = sampler.GetColors();
                var colCount = cols.Length;
                monitorColors2[i] = cols;
                monitorLightCounts[i] = colCount;
                texCount += colCount;
            }

            if (isAmbientEnable)
            {
                texCount += 1;
            }
            colors = new Color[texCount];
        }

        void Update()
        {
            var index = 0;
            for (var i = 0; i < lightCount; i++)
            {
                var light = lights[i];
                colors[index] = light.color * light.intensity;
                index++;
            }

            for (var i = 0; i < rendererCount; i++)
            {
                var renderer = emissiveRenderers[i];
                var mats = renderer.materials;
                var count = materialCounts[i];
                var emissiveFlags = materialEmissiveFlags[i];
                for(var j = 0; j < count; j++)
                {
                    if (emissiveFlags[j])
                    {
                        colors[index] = mats[j].GetColor("_EmissionColor");
                        index++;
                    }
                }
            }

            for (var i = 0; i < monitorCount; i++)
            {
                var cols = monitorColors2[i];
                var count = monitorLightCounts[i];
                for (var j = 0; j < count; j++)
                {
                    colors[index + j] = cols[j];
                }
                index += count;
            }

            if (isAmbientEnable)
            {
                colors[index] = Color.white * ambientIntensity;
            }

            foreach (var mat in compositeMaterials)
            {
                mat.SetColorArray("_Colors", colors);
            }
        }
    }
}