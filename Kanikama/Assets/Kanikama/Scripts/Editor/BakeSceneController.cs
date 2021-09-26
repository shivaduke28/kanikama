using Kanikama.EditorOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor
{
    public class BakeSceneController : IDisposable
    {
        readonly KanikamaSceneDescriptor sceneDescriptor;

        public List<KanikamaLight> KanikamaLights { get; } = new List<KanikamaLight>();
        public List<KanikamaEmissiveRenderer> KanikamaEmissiveRenderers { get; } = new List<KanikamaEmissiveRenderer>();
        public List<KanikamaMonitorData> KanikamaMonitors { get; } = new List<KanikamaMonitorData>();
        public bool IsKanikamaAmbientEnable => sceneDescriptor.IsAmbientEnable;


        readonly List<Light> nonKanikamaLights = new List<Light>();
        readonly Dictionary<GameObject, Material[]> nonKanikamaMaterialMaps = new Dictionary<GameObject, Material[]>();
        readonly List<ReflectionProbe> reflectionProbes = new List<ReflectionProbe>();
        readonly List<LightProbeGroup> lightProbeGroups = new List<LightProbeGroup>();

        float ambientIntensity;
        Color ambientLight;
        Color ambientSkyColor;
        Color ambientGroundColor;
        Color ambientEquatorColor;
        Material dummyMaterial;
        LightmapsMode lightmapsMode;


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
                    !sceneDescriptor.Lights.Contains(light))
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

            // kanikama monitors
            KanikamaMonitors.AddRange(sceneDescriptor.MonitorSetups.Select(x => new KanikamaMonitorData(x)));

            // non kanikama emissive renderers
            var allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
            foreach (var renderer in allRenderers)
            {
                if (sceneDescriptor.EmissiveRenderers.Contains(renderer)) continue;
                if (sceneDescriptor.MonitorSetups.Any(x => x.Contains(renderer))) continue;

                var flag = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                if (flag.HasFlag(StaticEditorFlags.ContributeGI))
                {
                    var sharedMaterials = renderer.sharedMaterials;

                    if (sharedMaterials.Any(x => !(x is null) && x.IsKeywordEnabled(KanikamaEmissiveMaterial.ShaderKeywordEmission)))
                    {
                        nonKanikamaMaterialMaps[renderer.gameObject] = sharedMaterials;
                        renderer.sharedMaterials = Enumerable.Repeat(dummyMaterial, sharedMaterials.Length).ToArray();
                    }
                }
            }

            // reflection probes
            reflectionProbes.AddRange(UnityEngine.Object.FindObjectsOfType<ReflectionProbe>().Where(x => x.gameObject.activeInHierarchy && x.enabled));
            // light probes
            lightProbeGroups.AddRange(UnityEngine.Object.FindObjectsOfType<LightProbeGroup>().Where(x => x.gameObject.activeInHierarchy && x.enabled));

            // ambient
            ambientIntensity = RenderSettings.ambientIntensity;
            ambientLight = RenderSettings.ambientLight;
            ambientSkyColor = RenderSettings.ambientSkyColor;
            ambientEquatorColor = RenderSettings.ambientEquatorColor;
            ambientGroundColor = RenderSettings.ambientGroundColor;

            // directional mode
            lightmapsMode = LightmapEditorSettings.lightmapsMode;
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

            foreach (var probe in reflectionProbes)
            {
                probe.enabled = false;
            }

            foreach (var probe in lightProbeGroups)
            {
                probe.enabled = false;
            }
        }

        public void OnAmbientBake()
        {
            RenderSettings.ambientIntensity = 1;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
        }

        public void TurnOffAmbient()
        {
            RenderSettings.ambientIntensity = 0f;
            RenderSettings.ambientLight = Color.black;
            RenderSettings.ambientSkyColor = Color.black;
            RenderSettings.ambientEquatorColor = Color.black;
            RenderSettings.ambientGroundColor = Color.black;
        }

        void RollbackAmbient()
        {
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
        }

        public void SetLightmapSettings(bool isDirectional)
        {
            if (isDirectional)
            {
                LightmapEditorSettings.lightmapsMode = LightmapsMode.CombinedDirectional;
            }
            else
            {
                LightmapEditorSettings.lightmapsMode = LightmapsMode.NonDirectional;
            }
        }

        public void Rollback()
        {
            RollbackNonKanikama();
            RollbackKanikama();
            RollbackLightmapSettings();
        }

        public void RollbackNonKanikama()
        {
            if (!IsKanikamaAmbientEnable)
            {
                RollbackAmbient();
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

            foreach (var probe in reflectionProbes)
            {
                probe.enabled = true;
            }

            foreach (var probe in lightProbeGroups)
            {
                probe.enabled = true;
            }
        }

        public void RollbackLightmapSettings()
        {
            LightmapEditorSettings.lightmapsMode = lightmapsMode;
        }

        public void RollbackKanikama()
        {
            if (IsKanikamaAmbientEnable)
            {
                RollbackAmbient();
            }

            foreach (var lightData in KanikamaLights)
            {
                lightData.RollBack();
            }

            foreach (var monitor in KanikamaMonitors)
            {
                monitor.RollBack();
            }

            foreach (var rendererData in KanikamaEmissiveRenderers)
            {
                rendererData.RollBack();
            }
        }

        public bool ValidateTexturePath(BakePath.TempTexturePath pathData)
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
                    return pathData.SubIndex < setUp.MainMonitor.gridRenderers.Count;
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
