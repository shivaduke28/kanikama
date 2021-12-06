using System;
using UnityEngine;

namespace Kanikama
{
    [Serializable]
    public class KanikamaLightMaterial : ILightSource
    {
        public static readonly int ShaderPropertyEmissionColor = Shader.PropertyToID(ShaderPropertyEmissionColorName);
        public const string ShaderPropertyEmissionColorName = "_EmissionColor";

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
            RemoveBakedEmissiveFlag(materialInstance);
        }

        public void OnBake()
        {
            AddBakedEmissiveFlag(materialInstance);
            materialInstance.SetColor(ShaderPropertyEmissionColor, Color.white);
        }

        public void Rollback()
        {
            if (materialInstance != null)
            {
                UnityEngine.Object.DestroyImmediate(materialInstance);
            }
        }

        public static bool IsBakedEmissive(Material mat)
        {
            return mat.globalIlluminationFlags.HasFlag(MaterialGlobalIlluminationFlags.BakedEmissive);
        }

        public static void RemoveBakedEmissiveFlag(Material mat)
        {
            var flags = mat.globalIlluminationFlags;
            flags &= ~MaterialGlobalIlluminationFlags.BakedEmissive;
            mat.globalIlluminationFlags = flags;
        }

        public static void AddBakedEmissiveFlag(Material mat)
        {
            var flags = mat.globalIlluminationFlags;
            flags |= MaterialGlobalIlluminationFlags.BakedEmissive;
            mat.globalIlluminationFlags = flags;
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