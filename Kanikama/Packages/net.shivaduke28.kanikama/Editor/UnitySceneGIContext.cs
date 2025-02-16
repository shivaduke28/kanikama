﻿using System;
using System.Collections.Generic;
using System.Linq;
using Kanikama.Editor.Utility;
using Kanikama.Utility;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Kanikama.Editor
{
    public sealed class UnitySceneGIContext
    {
        List<ObjectHandle<Light>> lights;
        List<ObjectHandle<RendererMaterialHolder>> renderers;
        List<ObjectHandle<LightProbeGroup>> lightProbeGroups;
        List<ObjectHandle<ReflectionProbe>> reflectionProbes;
        AmbientLight ambientLight;
        List<RendererWithShadowCastingMode> rendererWithShadowCastingModes;

        public static UnitySceneGIContext GetGIContext(Func<Object, bool> filter = null)
        {
            var context = new UnitySceneGIContext
            {
                ambientLight = new AmbientLight(),
                lights = Object.FindObjectsOfType<Light>()
                    .Where(l => l.IsContributeGI())
                    .Where(x => filter?.Invoke(x) ?? true)
                    .Select(l => new ObjectHandle<Light>(l))
                    .ToList(),
                renderers = Object.FindObjectsOfType<Renderer>()
                    .Where(x => filter?.Invoke(x) ?? true)
                    .Where(r => r.IsEmissiveAndContributeGI())
                    .Select(r => Kanikama.Utility.ExtensionMethods.GetOrAddComponent<RendererMaterialHolder>(r.gameObject))
                    .Select(h => new ObjectHandle<RendererMaterialHolder>(h))
                    .ToList(),
                lightProbeGroups = Object.FindObjectsOfType<LightProbeGroup>()
                    .Select(lg => new ObjectHandle<LightProbeGroup>(lg))
                    .ToList(),
                reflectionProbes = Object.FindObjectsOfType<ReflectionProbe>()
                    .Select(rp => new ObjectHandle<ReflectionProbe>(rp))
                    .ToList(),
                rendererWithShadowCastingModes = Object.FindObjectsOfType<Renderer>()
                    .Where(x => filter?.Invoke(x) ?? true)
                    .Where(r => r.gameObject.IsContributeGI())
                    .Select(r => new RendererWithShadowCastingMode(r))
                    .ToList(),
            };
            return context;
        }

        // NOTE: This method has a side effect to an active Scene.
        // Call for a copied scene.
        public void TurnOff()
        {
            foreach (var handle in lights)
            {
                handle.Value.intensity = 0;
            }

            foreach (var handle in renderers)
            {
                var materials = handle.Value.GetMaterials();
                foreach (var material in materials)
                {
                    material.RemoveBakedEmissiveFlag();
                }
            }

            foreach (var handle in lightProbeGroups)
            {
                handle.Value.enabled = false;
            }
            foreach (var handle in reflectionProbes)
            {
                handle.Value.enabled = false;
            }

            ambientLight.TurnOff();
        }

        public void SetCastShadowOff()
        {
            foreach (var renderer in rendererWithShadowCastingModes)
            {
                renderer.SetShadowCastingMode(ShadowCastingMode.Off);
            }
        }

        public void ClearCastShadow()
        {
            foreach (var renderer in rendererWithShadowCastingModes)
            {
                renderer.ClearShadowCastingMode();
            }
        }

        sealed class AmbientLight
        {
            float ambientIntensity;
            Color ambientLight;
            Color ambientSkyColor;
            Color ambientGroundColor;
            Color ambientEquatorColor;
            AmbientMode ambientMode;

            public float Intensity
            {
                get => RenderSettings.ambientIntensity;
                set => RenderSettings.ambientIntensity = value;
            }

            public Color Light
            {
                get => RenderSettings.ambientLight;
                set => RenderSettings.ambientLight = value;
            }

            public Color SkyColor
            {
                get => RenderSettings.ambientSkyColor;
                set => RenderSettings.ambientSkyColor = value;
            }

            public Color EquatorColor
            {
                get => RenderSettings.ambientEquatorColor;
                set => RenderSettings.ambientEquatorColor = value;
            }

            public Color GroundColor
            {
                get => RenderSettings.ambientGroundColor;
                set => RenderSettings.ambientGroundColor = value;
            }

            public AmbientMode Mode
            {
                get => RenderSettings.ambientMode;
                set => RenderSettings.ambientMode = value;
            }

            public void CacheCurrentRenderSettings()
            {
                ambientIntensity = RenderSettings.ambientIntensity;
                ambientLight = RenderSettings.ambientLight;
                ambientSkyColor = RenderSettings.ambientSkyColor;
                ambientEquatorColor = RenderSettings.ambientEquatorColor;
                ambientGroundColor = RenderSettings.ambientGroundColor;
            }

            public AmbientLight()
            {
                CacheCurrentRenderSettings();
            }

            public void TurnOff()
            {
                RenderSettings.ambientIntensity = 0f;
                RenderSettings.ambientLight = Color.black;
                RenderSettings.ambientSkyColor = Color.black;
                RenderSettings.ambientEquatorColor = Color.black;
                RenderSettings.ambientGroundColor = Color.black;
            }

            public void Revert()
            {
                RenderSettings.ambientIntensity = ambientIntensity;
                RenderSettings.ambientLight = ambientLight;
                RenderSettings.ambientSkyColor = ambientSkyColor;
                RenderSettings.ambientEquatorColor = ambientEquatorColor;
                RenderSettings.ambientGroundColor = ambientGroundColor;
            }
        }
    }
}
