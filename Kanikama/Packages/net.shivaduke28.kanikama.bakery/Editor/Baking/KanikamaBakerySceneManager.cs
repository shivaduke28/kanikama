#if BAKERY_INCLUDED
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.Baking.Bakery
{
    public class KanikamaBakerySceneManager : IKanikamaSceneManager
    {
        readonly KanikamaSceneDescriptor sceneDescriptor;
        public List<ObjectReference<LightSource>> LightSources { get; private set; }
        public List<ObjectReference<KanikamaLightSourceGroup>> LightSourceGroups { get; private set; }

        List<ObjectReference<BakeryPointLight>> nonKanikamaBakeryPointLights;
        List<ObjectReference<BakeryDirectLight>> nonKanikamaBakeryDirectLights;
        List<ObjectReference<BakerySkyLight>> nonKanikamaBakerySkyLights;
        List<ObjectReference<BakeryLightMesh>> nonKanikamaBakeryLightMeshes;

        readonly List<NonKanikamaRenderer> nonKanikamaRenderers = new List<NonKanikamaRenderer>();
        readonly List<ObjectReference<ReflectionProbe>> reflectionProbes = new List<ObjectReference<ReflectionProbe>>();
        readonly List<ObjectReference<LightProbeGroup>> lightProbeGroups = new List<ObjectReference<LightProbeGroup>>();

        ftRenderLightmap.RenderDirMode renderDirMode;

        public KanikamaBakerySceneManager(KanikamaSceneDescriptor sceneDescriptor)
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

            // non kanikama bakery lights
            nonKanikamaBakeryPointLights = Object.FindObjectsOfType<BakeryPointLight>()
                .Where(x => x.gameObject.activeInHierarchy && x.enabled)
                .Where(x => !IsKanikama(x))
                .Select(x => new ObjectReference<BakeryPointLight>(x))
                .ToList();
            nonKanikamaBakeryDirectLights = Object.FindObjectsOfType<BakeryDirectLight>()
                .Where(x => x.gameObject.activeInHierarchy && x.enabled)
                .Where(x => !IsKanikama(x))
                .Select(x => new ObjectReference<BakeryDirectLight>(x))
                .ToList();
            nonKanikamaBakerySkyLights = Object.FindObjectsOfType<BakerySkyLight>()
                .Where(x => x.gameObject.activeInHierarchy && x.enabled)
                .Where(x => !IsKanikama(x))
                .Select(x => new ObjectReference<BakerySkyLight>(x))
                .ToList();
            nonKanikamaBakeryLightMeshes = Object.FindObjectsOfType<BakeryLightMesh>()
                .Where(x => x.gameObject.activeInHierarchy && x.enabled)
                .Where(x => !IsKanikama(x))
                .Select(x => new ObjectReference<BakeryLightMesh>(x))
                .ToList();

            // non kanikama emissive renderers
            var allRenderers = Object.FindObjectsOfType<Renderer>();
            foreach (var renderer in allRenderers)
            {
                if (IsKanikama(renderer)) continue;

                if (renderer.GetComponent<BakeryLightMesh>() != null) continue;

                if (NonKanikamaRenderer.IsTarget(renderer))
                {
                    nonKanikamaRenderers.Add(new NonKanikamaRenderer(renderer));
                }
            }

            // reflection probes
            reflectionProbes.AddRange(Object.FindObjectsOfType<ReflectionProbe>()
                .Where(x => x.gameObject.activeInHierarchy && x.enabled)
                .Select(x => new ObjectReference<ReflectionProbe>(x)));
            // light probes
            lightProbeGroups.AddRange(Object.FindObjectsOfType<LightProbeGroup>()
                .Where(x => x.gameObject.activeInHierarchy && x.enabled)
                .Select(x => new ObjectReference<LightProbeGroup>(x)));

            // directional mode
            renderDirMode = ftRenderLightmap.renderDirMode;
        }

        public void TurnOff()
        {
            foreach (var light in nonKanikamaBakeryPointLights)
            {
                light.Value.gameObject.SetActive(false);
            }

            foreach (var light in nonKanikamaBakeryDirectLights)
            {
                light.Value.gameObject.SetActive(false);
            }

            foreach (var light in nonKanikamaBakerySkyLights)
            {
                light.Value.gameObject.SetActive(false);
            }

            foreach (var light in nonKanikamaBakeryLightMeshes)
            {
                light.Value.gameObject.SetActive(false);
            }

            foreach (var nonKanikamaRenderer in nonKanikamaRenderers)
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
                ftRenderLightmap.renderDirMode = ftRenderLightmap.RenderDirMode.DominantDirection;
            }
            else
            {
                ftRenderLightmap.renderDirMode = ftRenderLightmap.RenderDirMode.None;
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
            foreach (var light in nonKanikamaBakeryPointLights)
            {
                light.Value.gameObject.SetActive(true);
            }

            foreach (var light in nonKanikamaBakeryDirectLights)
            {
                light.Value.gameObject.SetActive(true);
            }

            foreach (var light in nonKanikamaBakerySkyLights)
            {
                light.Value.gameObject.SetActive(true);
            }

            foreach (var light in nonKanikamaBakeryLightMeshes)
            {
                light.Value.gameObject.SetActive(true);
            }

            foreach (var nonKanikama in nonKanikamaRenderers)
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
            ftRenderLightmap.renderDirMode = renderDirMode;
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
#endif