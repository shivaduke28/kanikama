using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.Editor
{
    public class KanikamaLight
    {
        float intensity;
        Color color;
        Light light;
        public bool Enabled { get => light.enabled; set => light.enabled = value; }
        public string Name => light.name;

        public KanikamaLight(Light light)
        {
            intensity = light.intensity;
            color = light.color;
            this.light = light;
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
            light.enabled = true;
        }
    }

    public class KanikamaEmissiveRenderer
    {
        readonly Renderer renderer;
        readonly Material[] sharedMaterials;
        readonly Material[] tmpMaterials;
        public List<KanikamaEmissiveMaterial> EmissiveMaterial { get; } = new List<KanikamaEmissiveMaterial>();
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
                    EmissiveMaterial.Add(matData);
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
            foreach (var matData in EmissiveMaterial)
            {
                matData.TurnOff();
            }
        }

        public void RollBack()
        {
            renderer.sharedMaterials = sharedMaterials;
            foreach (var matData in EmissiveMaterial)
            {
                matData.Dispose();
            }
            EmissiveMaterial.Clear();
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
}