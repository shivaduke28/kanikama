using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Kanikama.EditorOnly;

namespace Kanikama.Editor
{
    [Serializable]
    public class KanikamaSceneData : IDisposable
    {
        readonly KanikamaSceneDescriptor sceneDescriptor;
        public List<KanikamaLightData> kanikamaLightDatas = new List<KanikamaLightData>();
        public List<KanikamaRendererData> kanikamaRendererDatas = new List<KanikamaRendererData>();
        public List<KanikamaMonitorSetup> kanikamaMonitors => sceneDescriptor.kanikamaMonitors;
        public List<Light> nonKanikamaLights = new List<Light>();
        public List<Renderer> nonKanikamaEmissiveRenderers = new List<Renderer>();
        Dictionary<GameObject, Material[]> nonKanikamaMaterialMaps = new Dictionary<GameObject, Material[]>();
        float ambientIntensity;
        Material dummyMaterial;


        public KanikamaSceneData(KanikamaSceneDescriptor sceneDescriptor)
        {
            this.sceneDescriptor = sceneDescriptor;
        }

        public void LoadActiveScene()
        {
            // kanikama lights
            kanikamaLightDatas.AddRange(sceneDescriptor.kanikamaLights.Select(x => new KanikamaLightData(x)));

            // non kanikama lights
            var allLights = UnityEngine.Object.FindObjectsOfType<Light>();
            foreach (var light in allLights)
            {
                if (light.enabled && light.lightmapBakeType != LightmapBakeType.Realtime && !sceneDescriptor.kanikamaLights.Contains(light))
                {
                    nonKanikamaLights.Add(light);
                }
            }

            // kanikama emissive renderers
            kanikamaRendererDatas.AddRange(sceneDescriptor.kanikamaRenderers.Select(x => new KanikamaRendererData(x)));

            if (dummyMaterial is null)
            {
                dummyMaterial = new Material(Shader.Find(KanikamaBaker.DummyShaderName));
            }

            // non kanikama emissive renderers
            var allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
            foreach (var renderer in allRenderers)
            {
                if (sceneDescriptor.kanikamaRenderers.Contains(renderer)) continue;
                var flag = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                if (flag.HasFlag(StaticEditorFlags.LightmapStatic))
                {
                    var sharedMaterials = renderer.sharedMaterials;

                    if (sharedMaterials.Any(x => x.IsKeywordEnabled(KanikamaEmissiveMaterialData.ShaderKeywordEmission)))
                    {
                        nonKanikamaMaterialMaps[renderer.gameObject] = sharedMaterials;
                        renderer.sharedMaterials = Enumerable.Repeat(dummyMaterial, sharedMaterials.Length).ToArray();
                    }
                }
            }

            ambientIntensity = RenderSettings.ambientIntensity;
        }

        public void TurnOff()
        {
            RenderSettings.ambientIntensity = 0;
            foreach (var light in nonKanikamaLights)
            {
                light.enabled = false;
            }

            foreach (var lightData in kanikamaLightDatas)
            {
                lightData.TurnOff();
            }

            foreach (var rendererData in kanikamaRendererDatas)
            {
                rendererData.TurnOff();
            }

            foreach (var monitor in sceneDescriptor.kanikamaMonitors)
            {
                monitor.TurnOff();
            }
        }

        public void RollbackNonKanikama()
        {
            RenderSettings.ambientIntensity = ambientIntensity;
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
            foreach (var lightData in kanikamaLightDatas)
            {
                lightData.RollBack();
            }

            foreach (var monitor in sceneDescriptor.kanikamaMonitors)
            {
                monitor.RollBack();
            }

            foreach (var rendererData in kanikamaLightDatas)
            {
                rendererData.RollBack();
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
