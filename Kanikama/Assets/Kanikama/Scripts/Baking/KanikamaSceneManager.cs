using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Baking
{
    public class KanikamaSceneManager : IDisposable
    {
        readonly KanikamaSceneDescriptor sceneDescriptor;
        public List<ObjectReference<LightSource>> LightSources { get; private set; }
        public List<ObjectReference<KanikamaLightSourceGroup>> LightSourceGroups { get; private set; }
        readonly List<ObjectReference<Light>> nonKanikamaLights = new List<ObjectReference<Light>>();


        readonly Dictionary<GameObject, Material[]> nonKanikamaMaterialMaps = new Dictionary<GameObject, Material[]>();
        readonly List<ObjectReference<ReflectionProbe>> reflectionProbes = new List<ObjectReference<ReflectionProbe>>();
        readonly List<ObjectReference<LightProbeGroup>> lightProbeGroups = new List<ObjectReference<LightProbeGroup>>();

        Material dummyMaterial;
        LightmapsMode lightmapsMode;
        bool isKanikamaAmbientEnable;
        ObjectReference<KanikamaUnitySkyLight> tempAmbientLight;

        public KanikamaSceneManager(KanikamaSceneDescriptor sceneDescriptor)
        {
            this.sceneDescriptor = sceneDescriptor;
        }

        public void Initialize()
        {
            LightSources = sceneDescriptor.GetLightSources().Select(x => new ObjectReference<LightSource>(x)).ToList();
            LightSourceGroups = sceneDescriptor.GetLightSourceGroups().Select(x => new ObjectReference<KanikamaLightSourceGroup>(x)).ToList();
            foreach (var source in LightSources)
            {
                source.Value.OnBakeSceneStart();
            }
            foreach (var group in LightSourceGroups)
            {
                group.Value.OnBakeSceneStart();
            }

            isKanikamaAmbientEnable = IsKanikama(AmbientLightModel.Instance);
            if (!isKanikamaAmbientEnable)
            {
                var temp = new GameObject("TempAmbientLight").AddComponent<KanikamaUnitySkyLight>();
                tempAmbientLight = new ObjectReference<KanikamaUnitySkyLight>(temp);
            }

            // non kanikama lights
            var allLights = UnityEngine.Object.FindObjectsOfType<Light>();
            foreach (var light in allLights)
            {
                if (light.enabled &&
                    light.lightmapBakeType != LightmapBakeType.Realtime &&
                    !IsKanikama(light))
                {
                    nonKanikamaLights.Add(new ObjectReference<Light>(light));
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
            reflectionProbes.AddRange(UnityEngine.Object.FindObjectsOfType<ReflectionProbe>()
                .Where(x => x.gameObject.activeInHierarchy && x.enabled)
                .Select(x => new ObjectReference<ReflectionProbe>(x)));
            // light probes
            lightProbeGroups.AddRange(UnityEngine.Object.FindObjectsOfType<LightProbeGroup>()
                .Where(x => x.gameObject.activeInHierarchy && x.enabled)
                .Select(x => new ObjectReference<LightProbeGroup>(x)));

            // directional mode
            lightmapsMode = LightmapEditorSettings.lightmapsMode;
        }

        public void TurnOff()
        {
            foreach (var light in nonKanikamaLights)
            {
                light.Value.enabled = false;
            }

            foreach (var lightSource in LightSources)
            {
                lightSource.Value.TurnOff();
            }

            foreach (var group in LightSourceGroups)
            {
                foreach (var source in group.Value.GetLightSources())
                {
                    source.TurnOff();
                }
            }

            foreach (var probe in reflectionProbes)
            {
                probe.Value.enabled = false;
            }

            foreach (var probe in lightProbeGroups)
            {
                probe.Value.enabled = false;
            }

            if (!isKanikamaAmbientEnable)
            {
                tempAmbientLight.Value.TurnOff();
            }
        }

        bool IsKanikama(object obj)
        {
            return LightSources.Any(x => x.Value.Contains(obj)) ||
                LightSourceGroups.Any(x => x.Value.Contains(obj));
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
                tempAmbientLight.Value.Rollback();
            }

            foreach (var light in nonKanikamaLights)
            {
                light.Value.enabled = true;
            }

            foreach (var kvp in nonKanikamaMaterialMaps)
            {
                var go = kvp.Key;
                var renderer = go.GetComponent<Renderer>();
                renderer.sharedMaterials = kvp.Value;
            }

            foreach (var probe in reflectionProbes)
            {
                probe.Value.enabled = true;
            }

            foreach (var probe in lightProbeGroups)
            {
                probe.Value.enabled = true;
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
                source.Value.Rollback();
            }

            foreach (var group in LightSourceGroups)
            {
                group.Value.Rollback();
            }
        }

        public bool ValidateTexturePath(KanikamaPath.TempTexturePath pathData)
        {
            switch (pathData.Type)
            {
                case KanikamaPath.BakeTargetType.LightSource:
                    return pathData.ObjectIndex < LightSources.Count;
                case KanikamaPath.BakeTargetType.LightSourceGroup:
                    if (pathData.ObjectIndex >= LightSourceGroups.Count) return false;
                    var group = LightSourceGroups[pathData.ObjectIndex];
                    return pathData.SubIndex < group.Value.GetLightSources().Count;
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
                UnityEngine.Object.DestroyImmediate(tempAmbientLight.Value.gameObject);
            }
        }
    }
}
