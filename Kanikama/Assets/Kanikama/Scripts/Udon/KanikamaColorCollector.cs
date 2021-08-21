
using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    public class KanikamaColorCollector : UdonSharpBehaviour
    {
        [SerializeField] Light[] lights;
        [SerializeField] Renderer[] emissiveRenderers;
        [SerializeField] KanikamaColorSampler[] colorSamplers;
        [SerializeField] bool isAmbientEnable;
        [ColorUsage(false, true), SerializeField] Color ambientColor;

        [Space]
        [ColorUsage(false, true), SerializeField] Vector4[] colors; // linear

        public Vector4[] GetColors()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            return colors;
        }

        bool isInitialized;
        int frameCount;

        int size;
        int lightCount;

        int rendererCount;
        int[] materialCounts;
        bool[][] materialEmissiveFlags;

        int monitorCount;
        Color[][] monitorColors;
        int[] monitorLightCounts;

        void Initialize()
        {
            size = 0;

            // Ambient
            if (isAmbientEnable) size += 1;

            // Light
            lightCount = lights == null ? 0 : lights.Length;
            size += lightCount;

            // Monitor
            monitorCount = colorSamplers == null ? 0 : colorSamplers.Length;
            if (monitorCount > 0)
            {
                monitorColors = new Color[monitorCount][];
                monitorLightCounts = new int[monitorCount];
            }

            for (var i = 0; i < monitorCount; i++)
            {
                var sampler = colorSamplers[i];
                var cols = sampler.GetColors();
                var colCount = cols.Length;
                monitorColors[i] = cols;
                monitorLightCounts[i] = colCount;
                size += colCount;
            }

            // Renderer
            rendererCount = emissiveRenderers == null ? 0 : emissiveRenderers.Length;
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

            colors = new Vector4[size];
            isInitialized = true;
        }

        public void Collect()
        {
            if (frameCount >= Time.frameCount) return;
            frameCount = Time.frameCount;

            var index = 0;

            // Ambient
            if (isAmbientEnable)
            {
                // HDR (linear)
                colors[index] = ambientColor;
                index++;
            }

            // Light
            for (var i = 0; i < lightCount; i++)
            {
                var light = lights[i];
                // NOTE: depends on GraphicsSettings.lightsUseLinearIntensity
                colors[index] = light.color.linear * light.intensity;
                index++;
            }

            // Monitor
            for (var i = 0; i < monitorCount; i++)
            {
                var cols = monitorColors[i];
                var count = monitorLightCounts[i];
                for (var j = 0; j < count; j++)
                {
                    // monitorColors should be linear
                    colors[index] = cols[j];
                    index++;
                }
            }

            // Renderer
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
                        // HDR (linear)
                        colors[index] = mats[j].GetColor("_EmissionColor");
                        index++;
                    }
                }
            }
        }
    }
}