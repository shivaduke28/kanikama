using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Baking
{
    public class KanikamaUnitySceneManager : IKanikamaSceneManager
    {
        readonly KanikamaSceneDescriptor sceneDescriptor;
        public List<ObjectReference<LightSource>> LightSources { get; private set; }
        public List<ObjectReference<KanikamaLightSourceGroup>> LightSourceGroups { get; private set; }
        readonly List<ObjectReference<Light>> nonKanikamaLights = new List<ObjectReference<Light>>();

        readonly List<NonKanikamaRenderer> nonKanikamaRenderers = new List<NonKanikamaRenderer>();
        readonly List<ObjectReference<ReflectionProbe>> reflectionProbes = new List<ObjectReference<ReflectionProbe>>();
        readonly List<ObjectReference<LightProbeGroup>> lightProbeGroups = new List<ObjectReference<LightProbeGroup>>();

        LightmapsMode lightmapsMode;
        bool isKanikamaAmbientEnable;
        ObjectReference<KanikamaUnitySkyLight> tempAmbientLight;

        public KanikamaUnitySceneManager(KanikamaSceneDescriptor sceneDescriptor)
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
                temp.OnBakeSceneStart();
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

            // non kanikama emissive renderers
            var allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
            foreach (var renderer in allRenderers)
            {
                if (IsKanikama(renderer)) continue;

                if (NonKanikamaRenderer.IsTarget(renderer))
                {
                    nonKanikamaRenderers.Add(new NonKanikamaRenderer(renderer));
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

            foreach(var nonKanikamaRenderer in nonKanikamaRenderers)
            {
                nonKanikamaRenderer.TurnOff();
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

        public void SetDirectionalMode(bool isDirectional)
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
            RollbackDirectionalMode();
        }

        public void RollbackNonKanikama()
        {
            if (!isKanikamaAmbientEnable)
            {
                var value = tempAmbientLight.Value;
                if (value != null)
                {
                    value.Rollback();
                    Object.DestroyImmediate(value.gameObject);
                }
            }

            foreach (var light in nonKanikamaLights)
            {
                light.Value.enabled = true;
            }

            foreach(var nonKanikama in nonKanikamaRenderers)
            {
                nonKanikama.Rollback();
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

        public void RollbackDirectionalMode()
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
    }
}
