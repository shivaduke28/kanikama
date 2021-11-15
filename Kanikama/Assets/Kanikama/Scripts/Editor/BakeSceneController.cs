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
        public IReadOnlyList<IKanikamaLightSource> LightSources { get; private set; }
        public IReadOnlyList<IKanikamaLightSourceGroup> LightSourceGroups { get; private set; }
        readonly List<Light> nonKanikamaLights = new List<Light>();
        readonly Dictionary<GameObject, Material[]> nonKanikamaMaterialMaps = new Dictionary<GameObject, Material[]>();
        readonly List<ReflectionProbe> reflectionProbes = new List<ReflectionProbe>();
        readonly List<LightProbeGroup> lightProbeGroups = new List<LightProbeGroup>();

        Material dummyMaterial;
        LightmapsMode lightmapsMode;
        bool isKanikamaAmbientEnable;
        KanikamaAmbientLight tempAmbientLight;

        public BakeSceneController(KanikamaSceneDescriptor sceneDescriptor)
        {
            this.sceneDescriptor = sceneDescriptor;
        }

        public void Initialize()
        {
            LightSources = sceneDescriptor.GetLightSources();
            LightSourceGroups = sceneDescriptor.GetLightSourceGroups();
            foreach (var group in LightSourceGroups)
            {
                group.OnBakeSceneStart();
            }

            isKanikamaAmbientEnable = IsKanikama(AmbientLightModel.Instance);
            if (!isKanikamaAmbientEnable)
            {
                tempAmbientLight = new GameObject("TempAmbientLight").AddComponent<KanikamaAmbientLight>();
            }

            // non kanikama lights
            var allLights = UnityEngine.Object.FindObjectsOfType<Light>();
            foreach (var light in allLights)
            {
                if (light.enabled &&
                    light.lightmapBakeType != LightmapBakeType.Realtime &&
                    !IsKanikama(light))
                {
                    nonKanikamaLights.Add(light);
                }
            }

            if (dummyMaterial == null)
            {
                dummyMaterial = new Material(Shader.Find(Baker.ShaderName.Dummy));
            }

            // non kanikama emissive renderers
            var allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
            foreach (var renderer in allRenderers)
            {
                if (IsKanikama(renderer)) continue;

                var flag = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                if (flag.HasFlag(StaticEditorFlags.ContributeGI))
                {
                    var sharedMaterials = renderer.sharedMaterials;

                    if (sharedMaterials.Any(x => !(x is null) && KanikamaLightMaterial.IsTarget(x)))
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

            // directional mode
            lightmapsMode = LightmapEditorSettings.lightmapsMode;
        }

        public void TurnOff()
        {
            foreach (var light in nonKanikamaLights)
            {
                light.enabled = false;
            }

            foreach (var lightSource in LightSources)
            {
                lightSource.TurnOff();
            }

            foreach (var group in LightSourceGroups)
            {
                foreach (var source in group.GetLightSources())
                {
                    source.TurnOff();
                }
            }

            foreach (var probe in reflectionProbes)
            {
                probe.enabled = false;
            }

            foreach (var probe in lightProbeGroups)
            {
                probe.enabled = false;
            }

            if (!isKanikamaAmbientEnable)
            {
                tempAmbientLight.TurnOff();
            }
        }

        bool IsKanikama(object obj)
        {
            return LightSources.Any(x => x.Contains(obj)) ||
                LightSourceGroups.Any(x => x.Contains(obj) || x.GetLightSources().Any(y => y.Contains(obj)));
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
            if (!isKanikamaAmbientEnable)
            {
                tempAmbientLight.Rollback();
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
            foreach (var source in LightSources)
            {
                source.Rollback();
            }

            foreach (var group in LightSourceGroups)
            {
                foreach (var source in group.GetLightSources())
                {
                    source.Rollback();
                }
                group.Rollback();
            }
        }

        public bool ValidateTexturePath(BakePath.TempTexturePath pathData)
        {
            switch (pathData.Type)
            {
                case BakePath.BakeTargetType.LightSource:
                    return pathData.ObjectIndex < LightSources.Count;
                case BakePath.BakeTargetType.LightSourceGroup:
                    if (pathData.ObjectIndex >= LightSourceGroups.Count) return false;
                    var group = LightSourceGroups[pathData.ObjectIndex];
                    return pathData.SubIndex < group.GetLightSources().Count;
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
            if (tempAmbientLight != null)
            {
                UnityEngine.Object.DestroyImmediate(tempAmbientLight.gameObject);
            }
        }
    }
}
