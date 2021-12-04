
using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    [RequireComponent(typeof(Camera)), UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KanikamaColorCollector : UdonSharpBehaviour
    {
        [SerializeField] Light[] lights;
        [SerializeField] Renderer[] emissiveRenderers;
        [SerializeField] KanikamaCamera[] kanikamaCameras;

        [Space]
        [Range(0, 20f)] public float intensity = 1f;
        [SerializeField, HideInInspector] Vector4[] colors; // linear

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

            // Light
            lightCount = lights == null ? 0 : lights.Length;
            size += lightCount;

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
                    var isEmissive = ((byte)mats[j].globalIlluminationFlags & (byte)MaterialGlobalIlluminationFlags.BakedEmissive) == (byte)MaterialGlobalIlluminationFlags.BakedEmissive;
                    flags[j] = isEmissive;
                    if (isEmissive)
                    {
                        size++;
                    }
                }
                materialEmissiveFlags[i] = flags;
            }

            // Monitor
            monitorCount = kanikamaCameras == null ? 0 : kanikamaCameras.Length;
            if (monitorCount > 0)
            {
                monitorColors = new Color[monitorCount][];
                monitorLightCounts = new int[monitorCount];
            }

            for (var i = 0; i < monitorCount; i++)
            {
                var kanikamaCamera = kanikamaCameras[i];
                var cols = kanikamaCamera.GetColors();
                var colCount = cols.Length;
                monitorColors[i] = cols;
                monitorLightCounts[i] = colCount;
                size += colCount;
            }

            colors = new Vector4[size];
            isInitialized = true;
        }

        // Note:
        // Colors are updated on OnPreCull in every frame,
        // KanikamaProviders (and other scripts) attached to the same GameObject should use them on or after OnPreRender.
        void OnPreCull()
        {
            if (frameCount >= Time.frameCount) return;
            frameCount = Time.frameCount;

            var index = 0;

            // 1. Light (include Ambient)
            for (var i = 0; i < lightCount; i++)
            {
                var light = lights[i];
                // NOTE: depends on GraphicsSettings.lightsUseLinearIntensity
                colors[index] = light.color.linear * light.intensity * intensity;
                index++;
            }

            // 2. Renderer
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
                        colors[index] = mats[j].GetColor("_EmissionColor") * intensity;
                        index++;
                    }
                }
            }

            // 3. Monitor
            for (var i = 0; i < monitorCount; i++)
            {
                var cols = monitorColors[i];
                var count = monitorLightCounts[i];
                for (var j = 0; j < count; j++)
                {
                    // monitorColors should be linear
                    colors[index] = cols[j] * intensity;
                    index++;
                }
            }

            // 4. Others (You can add custom udon scripts here)
        }
    }
}