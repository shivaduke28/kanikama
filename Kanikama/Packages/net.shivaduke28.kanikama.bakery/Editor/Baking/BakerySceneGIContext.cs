using System;
using System.Collections.Generic;
using System.Linq;
using Kanikama.Utility;
using Kanikama.Editor.Baking;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Kanikama.Bakery.Editor.Baking
{
    public sealed class BakerySceneGIContext : IDisposable
    {
        List<ObjectHandle<BakeryDirectLight>> bakeryDirectLights;
        List<ObjectHandle<BakeryPointLight>> bakeryPointLights;
        List<ObjectHandle<BakeryLightMesh>> bakeryLightMeshes;

        List<ObjectHandle<BakerySkyLight>> bakerySkyLight;
        List<ObjectHandle<RendererMaterialHolder>> renderers;

        List<ObjectHandle<LightProbeGroup>> lightProbeGroups;
        List<ObjectHandle<ReflectionProbe>> reflectionProbes;

        List<RendererWithShadowCastingMode> rendererWithShadowCastingModes;

        public static BakerySceneGIContext GetContext(Func<Object, bool> filter = null)
        {
            var context = new BakerySceneGIContext
            {
                bakeryDirectLights = Object.FindObjectsOfType<BakeryDirectLight>()
                    .Where(x => filter?.Invoke(x) ?? true)
                    .Select(l => new ObjectHandle<BakeryDirectLight>(l))
                    .ToList(),
                bakeryPointLights = Object.FindObjectsOfType<BakeryPointLight>()
                    .Where(x => filter?.Invoke(x) ?? true)
                    .Select(l => new ObjectHandle<BakeryPointLight>(l))
                    .ToList(),
                bakeryLightMeshes = Object.FindObjectsOfType<BakeryLightMesh>()
                    .Where(x => filter?.Invoke(x) ?? true)
                    .Select(l => new ObjectHandle<BakeryLightMesh>(l))
                    .ToList(),
                bakerySkyLight = Object.FindObjectsOfType<BakerySkyLight>()
                    .Where(x => filter?.Invoke(x) ?? true)
                    .Select(l => new ObjectHandle<BakerySkyLight>(l))
                    .ToList(),
                lightProbeGroups = Object.FindObjectsOfType<LightProbeGroup>()
                    .Where(x => filter?.Invoke(x) ?? true)
                    .Select(lg => new ObjectHandle<LightProbeGroup>(lg))
                    .ToList(),
                reflectionProbes = Object.FindObjectsOfType<ReflectionProbe>()
                    .Where(x => filter?.Invoke(x) ?? true)
                    .Select(rp => new ObjectHandle<ReflectionProbe>(rp))
                    .ToList(),
                renderers = Object.FindObjectsOfType<Renderer>()
                    .Where(x => filter?.Invoke(x) ?? true)
                    .Where(r => r.IsEmissiveAndContributeGI())
                    .Select(r => r.gameObject.GetOrAddComponent<RendererMaterialHolder>())
                    .Select(h => new ObjectHandle<RendererMaterialHolder>(h))
                    .ToList(),
                rendererWithShadowCastingModes = Object.FindObjectsOfType<Renderer>()
                    .Where(x => filter?.Invoke(x) ?? true)
                    .Where(r => r.gameObject.IsContributeGI())
                    .Select(r => new RendererWithShadowCastingMode(r))
                    .ToList(),
            };
            return context;
        }

        public void TurnOff()
        {
            foreach (var handle in bakeryDirectLights)
            {
                handle.Value.enabled = false;
            }
            foreach (var handle in bakeryPointLights)
            {
                handle.Value.enabled = false;
            }
            foreach (var handle in bakeryLightMeshes)
            {
                handle.Value.enabled = false;
            }
            foreach (var handle in bakerySkyLight)
            {
                handle.Value.enabled = false;
            }
            foreach (var handle in lightProbeGroups)
            {
                handle.Value.enabled = false;
            }
            foreach (var handle in reflectionProbes)
            {
                handle.Value.enabled = false;
            }
            foreach (var handle in renderers)
            {
                var materials = handle.Value.GetMaterials();
                foreach (var material in materials)
                {
                    material.RemoveBakedEmissiveFlag();
                }
            }
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

        public void Dispose()
        {
            if (renderers != null)
            {
                foreach (var holder in renderers)
                {
                    if (holder.Value == null) continue;
                    holder.Value.Clear();
                    Object.DestroyImmediate(holder.Value);
                }
            }
        }
    }
}
