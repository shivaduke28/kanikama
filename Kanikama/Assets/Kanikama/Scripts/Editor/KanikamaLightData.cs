using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.Editor
{
    public class KanikamaLightData
    {
        float intensity;
        Color color;
        Light light;
        public bool Enabled { get => light.enabled; set => light.enabled = value; }

        public KanikamaLightData(Light light)
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

    public class KanikamaRendererData
    {
        readonly Renderer renderer;
        readonly Material[] sharedMaterials;
        readonly Material[] tmpMaterials;
        public List<KanikamaEmissiveMaterialData> EmissiveMaterialDatas { get; } = new List<KanikamaEmissiveMaterialData>();

        public KanikamaRendererData(Renderer renderer)
        {
            this.renderer = renderer;
            sharedMaterials = renderer.sharedMaterials;

            var count = sharedMaterials.Length;
            tmpMaterials = new Material[count];

            for (var i = 0; i < count; i++)
            {
                var mat = sharedMaterials[i];
                Material tmp;
                if (mat.IsKeywordEnabled(KanikamaEmissiveMaterialData.ShaderKeywordEmission))
                {
                    tmp = UnityEngine.Object.Instantiate(mat);
                    var matData = new KanikamaEmissiveMaterialData(tmp);
                    EmissiveMaterialDatas.Add(matData);
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
            foreach(var matData in EmissiveMaterialDatas)
            {
                matData.TurnOff();
            }
        }

        public void RollBack()
        {
            renderer.sharedMaterials = sharedMaterials;
            foreach (var matData in EmissiveMaterialDatas)
            {
                matData.Dispose();
            }
            EmissiveMaterialDatas.Clear();
        }
    }

    public class KanikamaEmissiveMaterialData : IDisposable
    {
        public static readonly string ShaderKeywordEmission = "_EMISSION";
        public static readonly int ShaderPropertyEmissionColor = Shader.PropertyToID("_EmissionColor");

        readonly Material material;

        public KanikamaEmissiveMaterialData(Material material)
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