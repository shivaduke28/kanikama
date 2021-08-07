using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Kanikama.EditorOnly;

namespace Kanikama.Editor
{
    public class BakeSceneController : IDisposable
    {
        readonly KanikamaSceneDescriptor sceneDescriptor;

        public List<KanikamaLight> KanikamaLights { get; } = new List<KanikamaLight>();
        public List<KanikamaEmissiveRenderer> KanikamaEmissiveRenderers { get; } = new List<KanikamaEmissiveRenderer>();
        public List<KanikamaMonitorSetup> KanikamaMonitors => sceneDescriptor.MonitorSetups;
        public bool IsKanikamaAmbientEnable => sceneDescriptor.IsAmbientEnable;


        readonly List<Light> nonKanikamaLights = new List<Light>();
        readonly Dictionary<GameObject, Material[]> nonKanikamaMaterialMaps = new Dictionary<GameObject, Material[]>();

        float ambientIntensity;
        Material dummyMaterial;

        public BakeSceneController(KanikamaSceneDescriptor sceneDescriptor)
        {
            this.sceneDescriptor = sceneDescriptor;
        }

        public void Initialize()
        {
            // kanikama lights
            KanikamaLights.AddRange(sceneDescriptor.Lights.Select(x => new KanikamaLight(x)));

            // non kanikama lights
            var allLights = UnityEngine.Object.FindObjectsOfType<Light>();
            foreach (var light in allLights)
            {
                if (light.enabled &&
                    light.lightmapBakeType != LightmapBakeType.Realtime &&
                    !sceneDescriptor.Lights.Contains(light) &&
                    !sceneDescriptor.MonitorSetups.Any(x => x.Lights.Contains(light)))
                {
                    nonKanikamaLights.Add(light);
                }
            }

            // kanikama emissive renderers
            KanikamaEmissiveRenderers.AddRange(sceneDescriptor.EmissiveRenderers.Select(x => new KanikamaEmissiveRenderer(x)));

            if (dummyMaterial is null)
            {
                dummyMaterial = new Material(Shader.Find(Baker.ShaderName.Dummy));
            }

            // non kanikama emissive renderers
            var allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
            foreach (var renderer in allRenderers)
            {
                if (sceneDescriptor.EmissiveRenderers.Contains(renderer)) continue;
                if (sceneDescriptor.MonitorSetups.Any(x => x.Renderer == renderer)) continue;

                var flag = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                if (flag.HasFlag(StaticEditorFlags.ContributeGI))
                {
                    var sharedMaterials = renderer.sharedMaterials;

                    if (sharedMaterials.Any(x => x.IsKeywordEnabled(KanikamaEmissiveMaterial.ShaderKeywordEmission)))
                    {
                        nonKanikamaMaterialMaps[renderer.gameObject] = sharedMaterials;
                        renderer.sharedMaterials = Enumerable.Repeat(dummyMaterial, sharedMaterials.Length).ToArray();
                    }
                }
            }

            // ambient
            ambientIntensity = RenderSettings.ambientIntensity;
        }

        public void TurnOff()
        {
            TurnOffAmbient();
            foreach (var light in nonKanikamaLights)
            {
                light.enabled = false;
            }

            foreach (var light in KanikamaLights)
            {
                light.TurnOff();
            }

            foreach (var renderer in KanikamaEmissiveRenderers)
            {
                renderer.TurnOff();
            }

            foreach (var monitor in KanikamaMonitors)
            {
                monitor.TurnOff();
            }
        }

        public void OnAmbientBake()
        {
            RenderSettings.ambientIntensity = 1;
        }

        public void TurnOffAmbient()
        {
            RenderSettings.ambientIntensity = 0f;
        }

        public void Rollback()
        {
            RollbackNonKanikama();
            RollbackKanikama();
        }

        public void RollbackNonKanikama()
        {
            if (!IsKanikamaAmbientEnable)
            {
                RenderSettings.ambientIntensity = ambientIntensity;
            }
            foreach (var light in nonKanikamaLights)
            {
                light.enabled = true;
            }

            foreach (var kvp in nonKanikamaMaterialMaps)
            {
                var go = kvp.Key;
                var renderer = go.GetComponent<Renderer>();
                renderer.sharedMaterials = kvp.Value;
            }
        }

        public void RollbackKanikama()
        {
            if (IsKanikamaAmbientEnable)
            {
                RenderSettings.ambientIntensity = ambientIntensity;
            }

            foreach (var lightData in KanikamaLights)
            {
                lightData.RollBack();
            }

            foreach (var monitor in sceneDescriptor.MonitorSetups)
            {
                monitor.RollBack();
            }

            foreach (var rendererData in KanikamaEmissiveRenderers)
            {
                rendererData.RollBack();
            }
        }

        public bool VaidateTexturePath(BakePath.TempTexturePath pathData)
        {
            switch (pathData.Type)
            {
                case BakePath.BakeTargetType.Ambient:
                    return sceneDescriptor.IsAmbientEnable;
                case BakePath.BakeTargetType.Light:
                    return pathData.ObjectIndex < sceneDescriptor.Lights.Count;
                case BakePath.BakeTargetType.Moitor:
                    if (pathData.ObjectIndex >= sceneDescriptor.MonitorSetups.Count) return false;
                    var setUp = sceneDescriptor.MonitorSetups[pathData.ObjectIndex];
                    return pathData.SubIndex < setUp.Lights.Count;
                case BakePath.BakeTargetType.Renderer:
                    if (pathData.ObjectIndex >= sceneDescriptor.EmissiveRenderers.Count) return false;
                    var renderer = KanikamaEmissiveRenderers[pathData.ObjectIndex];
                    return pathData.SubIndex < renderer.EmissiveMaterials.Count;
                default:
                    return false;
            }
        }

        public void Dispose()
        {
            if (dummyMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(dummyMaterial);
            }
        }
    }
}
