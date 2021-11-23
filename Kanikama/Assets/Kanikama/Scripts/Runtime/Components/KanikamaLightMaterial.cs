using System;
using UnityEngine;

namespace Kanikama
{
    [Serializable]
    public class KanikamaLightMaterial : ILightSource
    {
        public static readonly string ShaderKeywordEmission = "_EMISSION";
        public static readonly int ShaderPropertyEmissionColor = Shader.PropertyToID("_EmissionColor");

        readonly Material material;
        public string Name => material.name;


        public KanikamaLightMaterial(Material material)
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

        public void Rollback()
        {
            UnityEngine.Object.DestroyImmediate(material);
        }

        public static bool IsTarget(Material mat)
        {
            return mat.IsKeywordEnabled(KanikamaLightMaterial.ShaderKeywordEmission);
        }

        public bool Contains(object obj) => false;

        public void OnBakeSceneStart()
        {
        }
    }
}