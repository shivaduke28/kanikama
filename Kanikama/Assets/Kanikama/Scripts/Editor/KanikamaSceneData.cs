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
        public KanikamaSceneDescriptor sceneDescriptor;
        public List<KanikamaLightData> kanikamaLightData = new List<KanikamaLightData>();
        public List<Light> nonKanikamaLights = new List<Light>();
        public List<Renderer> nonKanikamaEmissiveRenderers = new List<Renderer>();
        Dictionary<GameObject, Material[]> materialMap = new Dictionary<GameObject, Material[]>();
        public float ambientIntensity;

        private Material dummyMaterial;

        public void LoadActiveScene()
        {
            var allLights = UnityEngine.Object.FindObjectsOfType<Light>();
            sceneDescriptor = UnityEngine.Object.FindObjectOfType<KanikamaSceneDescriptor>();
            if (sceneDescriptor is null)
            {
                throw new Exception($"Sceneに{typeof(KanikamaSceneDescriptor).Name}オブジェクトが存在しません");
            }

            kanikamaLightData.AddRange(sceneDescriptor.kanikamaLights.Select(x => new KanikamaLightData(x)));
            foreach (var light in allLights)
            {
                if (light.enabled && light.lightmapBakeType != LightmapBakeType.Realtime && !sceneDescriptor.kanikamaLights.Contains(light))
                {
                    nonKanikamaLights.Add(light);
                }
            }

            // TODO: Emissive Material

            if (dummyMaterial is null)
            {
                dummyMaterial = new Material(Shader.Find(KanikamaBaker.DummyShaderName));
            }

            var allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
            foreach (var renderer in allRenderers)
            {
                var flag = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                if (flag.HasFlag(StaticEditorFlags.LightmapStatic))
                {
                    var sharedMaterials = renderer.sharedMaterials;

                    if (sharedMaterials.Any(x => x.IsKeywordEnabled("_EMISSION")))
                    {
                        materialMap[renderer.gameObject] = sharedMaterials;
                        renderer.sharedMaterials = Enumerable.Repeat(dummyMaterial, sharedMaterials.Length).ToArray();
                    }
                }
            }



            ambientIntensity = RenderSettings.ambientIntensity;
        }

        public void SetupForBake()
        {
            RenderSettings.ambientIntensity = 0;
            foreach (var light in nonKanikamaLights)
            {
                light.enabled = false;
            }

            foreach (var lightData in kanikamaLightData)
            {
                lightData.OnPreBake();
            }

            foreach (var monitor in sceneDescriptor.kanikamaMonitors)
            {
                monitor.OnPreBake();
            }
        }

        public void RollbackNonKanikama()
        {
            RenderSettings.ambientIntensity = ambientIntensity;
            foreach (var light in nonKanikamaLights)
            {
                light.enabled = true;
            }

            foreach(var kvp in materialMap)
            {
                var go = kvp.Key;
                var renderer = go.GetComponent<Renderer>();
                renderer.sharedMaterials = kvp.Value;
            }
        }

        public void RollbackKanikama()
        {

            foreach (var lightData in kanikamaLightData)
            {
                lightData.OnPostBake();
            }

            foreach (var monitor in sceneDescriptor.kanikamaMonitors)
            {
                monitor.OnPostBake();
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
