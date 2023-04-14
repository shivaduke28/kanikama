﻿using System;
using System.Collections.Generic;
using System.Linq;
using Kanikama.Core;
using Kanikama.Core.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Bakery.Editor
{
    public sealed class BakerySceneGIContext : IDisposable
    {
        List<ObjectHandle<BakeryDirectLight>> bakeryDirectLights;
        List<ObjectHandle<BakeryPointLight>> bakeryPointLights;
        List<ObjectHandle<BakeryLightMesh>> bakeryLightMeshes;

        List<ObjectHandle<BakerySkyLight>> bakerySkyLight;
        List<ObjectHandle<RendererMaterialInstanceHolder>> renderers;

        List<ObjectHandle<LightProbeGroup>> lightProbeGroups;
        List<ObjectHandle<ReflectionProbe>> reflectionProbes;

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
                    .Where(KanikamaEditorUtility.IsContributeGI)
                    .Select(r => KanikamaRuntimeUtility.GetOrAddComponent<RendererMaterialInstanceHolder>(r.gameObject))
                    .Select(h => new ObjectHandle<RendererMaterialInstanceHolder>(h))
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
                    KanikamaRuntimeUtility.RemoveBakedEmissiveFlag(material);
                }
            }
        }

        public void Dispose()
        {
            var materialHolders = Object.FindObjectsOfType<RendererMaterialInstanceHolder>();
            foreach (var holder in materialHolders)
            {
                holder.Dispose();
                Object.DestroyImmediate(holder);
            }
        }
    }
}