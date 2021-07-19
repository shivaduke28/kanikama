using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Kanikama.EditorOnly;

namespace Kanikama.Editor
{
    [Serializable]
    public class KanikamaSceneData
    {
        public KanikamaSceneDescriptor sceneDescriptor;
        public List<KanikamaLightData> kanikamaLightData = new List<KanikamaLightData>();
        public List<Light> nonKanikamaLights = new List<Light>();
        public List<Renderer> nonKanikamaEmissiveRenderers = new List<Renderer>();
        public Dictionary<GameObject, Material> materialMap = new Dictionary<GameObject, Material>();
        public float ambientIntensity;

        public void LoadActiveScene()
        {
            var allLights = UnityEngine.Object.FindObjectsOfType<Light>();
            sceneDescriptor = UnityEngine.Object.FindObjectOfType<KanikamaSceneDescriptor>();
            if (sceneDescriptor is null)
            {
                throw new System.Exception($"Sceneに{typeof(KanikamaSceneDescriptor).Name}オブジェクトが存在しません");

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

            //var allRenderers = Object.FindObjectsOfType<Renderer>();
            //foreach (var renderer in allRenderers)
            //{
            //    var flag = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
            //    if (flag.HasFlag(StaticEditorFlags.LightmapStatic))
            //    {
            //        var mat = renderer.sharedMaterial;
            //        if (mat.IsKeywordEnabled("_EMISSION"))
            //        {
            //        }
            //    }

            //}



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
                lightData.BeDefault();
                lightData.Enabled = false;
            }

            foreach(var monitor in sceneDescriptor.kanikamaMonitors)
            {
                monitor.OnPreBake();
            }
        }

        public void Rollback()
        {
            RenderSettings.ambientIntensity = ambientIntensity;
            foreach (var light in nonKanikamaLights)
            {
                light.enabled = true;
            }

            foreach (var lightData in kanikamaLightData)
            {
                lightData.Rollback();
            }

            foreach (var monitor in sceneDescriptor.kanikamaMonitors)
            {
                monitor.OnPostBake();
            }
        }
    }
}
