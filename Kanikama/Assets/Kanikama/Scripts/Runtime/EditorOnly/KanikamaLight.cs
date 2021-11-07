#if UNITY_EDITOR && !COMPILER_UDONSHARP

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.EditorOnly
{
    public class KanikamaLight
    {
        float intensity;
        Color color;
        Light light;
        bool enabled;
        public string Name => light.name;

        public KanikamaLight(Light light)
        {
            intensity = light.intensity;
            color = light.color;
            this.light = light;
            enabled = light.enabled;
        }

        public void TurnOff()
        {
            light.enabled = false;
        }

        public void OnBake()
        {
            intensity = light.intensity;
            color = light.color;

            light.color = Color.white;
            light.intensity = 1f;
            light.enabled = true;
        }

        public void RollBack()
        {
            light.intensity = intensity;
            light.color = color;
            light.enabled = enabled;
        }
    }

    public class KanikamaEmissiveRenderer
    {
        readonly Renderer renderer;
        readonly Material[] sharedMaterials;
        readonly Material[] tmpMaterials;
        public List<KanikamaEmissiveMaterial> EmissiveMaterials { get; } = new List<KanikamaEmissiveMaterial>();
        public string Name => renderer.name;

        public KanikamaEmissiveRenderer(Renderer renderer)
        {
            this.renderer = renderer;
            sharedMaterials = renderer.sharedMaterials;

            var count = sharedMaterials.Length;
            tmpMaterials = new Material[count];

            for (var i = 0; i < count; i++)
            {
                var mat = sharedMaterials[i];
                Material tmp;
                if (mat.IsKeywordEnabled(KanikamaEmissiveMaterial.ShaderKeywordEmission))
                {
                    tmp = UnityEngine.Object.Instantiate(mat);
                    var matData = new KanikamaEmissiveMaterial(tmp);
                    EmissiveMaterials.Add(matData);
                }
                else
                {
                    tmp = mat;
                }
                tmpMaterials[i] = tmp;
            }
            renderer.sharedMaterials = tmpMaterials;
        }

        public void TurnOff()
        {
            foreach (var matData in EmissiveMaterials)
            {
                matData.TurnOff();
            }
        }

        public void RollBack()
        {
            renderer.sharedMaterials = sharedMaterials;
            foreach (var matData in EmissiveMaterials)
            {
                matData.Dispose();
            }
            EmissiveMaterials.Clear();
        }
    }

    public class KanikamaEmissiveMaterial : IDisposable
    {
        public static readonly string ShaderKeywordEmission = "_EMISSION";
        public static readonly int ShaderPropertyEmissionColor = Shader.PropertyToID("_EmissionColor");

        readonly Material material;
        public string Name => material.name;

        public KanikamaEmissiveMaterial(Material material)
        {
            this.material = material;
        }

        public void TurnOff()
        {
            material.DisableKeyword(ShaderKeywordEmission);
        }

        public void OnBake()
        {
            material.EnableKeyword(ShaderKeywordEmission);
            material.SetColor(ShaderKeywordEmission, Color.white);
        }

        public void Dispose()
        {
            UnityEngine.Object.DestroyImmediate(material);
        }
    }

    public class EmissiveMaterialGroup : IDisposable
    {
        public List<KanikamaEmissiveMaterial> EmissiveMaterials { get; } = new List<KanikamaEmissiveMaterial>();

        public void OnBake()
        {
            foreach (var mat in EmissiveMaterials)
            {
                mat.OnBake();
            }
        }

        public void TurnOff()
        {
            foreach (var mat in EmissiveMaterials)
            {
                mat.TurnOff();
            }
        }

        public void Dispose()
        {
            foreach (var mat in EmissiveMaterials)
            {
                mat.Dispose();
            }
        }
    }


    public class KanikamaMonitorData
    {
        readonly KanikamaMonitorSetup setup;
        readonly Material cachedMaterial;
        public List<EmissiveMaterialGroup> MaterialGroups { get; } = new List<EmissiveMaterialGroup>();
        public string Name => setup.MainMonitor.name;

        public KanikamaMonitorData(KanikamaMonitorSetup setup)
        {
            this.setup = setup;
            var count = setup.MainMonitor.gridRenderers.Count;
            cachedMaterial = setup.MainMonitor.gridRenderers[0].sharedMaterial;

            for (var i = 0; i < count; i++)
            {
                var materialGroup = new EmissiveMaterialGroup();
                foreach (var monitor in setup.Monitors)
                {
                    var gridRenderer = monitor.gridRenderers[i];
                    var tmp = UnityEngine.Object.Instantiate(gridRenderer.sharedMaterial);
                    gridRenderer.sharedMaterial = tmp;
                    materialGroup.EmissiveMaterials.Add(new KanikamaEmissiveMaterial(tmp));
                }
                MaterialGroups.Add(materialGroup);
            }
        }

        public void TurnOff()
        {
            setup.TurnOff();
            foreach (var matData in MaterialGroups)
            {
                matData.TurnOff();
            }
        }

        public void OnBake()
        {
            setup.OnBake();
        }

        public void RollBack()
        {
            setup.RollBack();
            foreach(var monitor in setup.Monitors)
            {
                foreach(var renderer in monitor.gridRenderers)
                {
                    renderer.sharedMaterial = cachedMaterial;
                }
            }
            foreach (var matData in MaterialGroups)
            {
                matData.Dispose();
            }
            MaterialGroups.Clear();
        }
    }
}

#endif