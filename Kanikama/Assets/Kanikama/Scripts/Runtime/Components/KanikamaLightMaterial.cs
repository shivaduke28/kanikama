using System;
using UnityEngine;

namespace Kanikama
{
    [Serializable]
    public class KanikamaLightMaterial : ILightSource
    {
        public static readonly string ShaderKeywordEmission = "_EMISSION";
        public static readonly int ShaderPropertyEmissionColor = Shader.PropertyToID("_EmissionColor");

        [SerializeField] int index;
        [SerializeField] Material material;
        [SerializeField, HideInInspector] Material materialInstance;
        public Material MaterialInstance => materialInstance;
        public string Name => material.name;
        public int Index => index;

        public KanikamaLightMaterial(int index, Material material)
        {
            this.index = index;
            this.material = material;
        }

        public void TurnOff()
        {
            materialInstance.DisableKeyword(ShaderKeywordEmission);
        }

        public void OnBake()
        {
            materialInstance.SetColor(ShaderPropertyEmissionColor, Color.white);
            materialInstance.EnableKeyword(ShaderKeywordEmission);
        }

        public void Rollback()
        {
            if (materialInstance != null)
            {
                UnityEngine.Object.DestroyImmediate(materialInstance);
            }
        }

        public static bool IsTarget(Material mat)
        {
            return mat.IsKeywordEnabled(ShaderKeywordEmission);
        }

        public bool Contains(object obj) => false;

        public void OnBakeSceneStart()
        {
            if (materialInstance != null)
            {
                UnityEngine.Object.DestroyImmediate(materialInstance);
            }
            materialInstance = UnityEngine.Object.Instantiate(material);
        }
    }
}