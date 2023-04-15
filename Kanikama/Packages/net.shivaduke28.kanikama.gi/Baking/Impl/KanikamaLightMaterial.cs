using Kanikama.Core;
using UnityEngine;

namespace Kanikama.GI.Baking.Impl
{
    [RequireComponent(typeof(Renderer))]
    [AddComponentMenu("Kanikama/GI/Baking.KanikamaLightMaterial")]
    public sealed class KanikamaLightMaterial : BakeTarget
    {
        [SerializeField] new Renderer renderer;
        [SerializeField] int materialIndex;
        [SerializeField] string propertyName = "_EmissionColor";
        [SerializeField] Material original;
        [SerializeField] Material instance;

        void OnValidate()
        {
            renderer = GetComponent<Renderer>();
        }

        public override void Initialize()
        {
            gameObject.SetActive(true);
            renderer.enabled = true;
            var sharedMaterials = renderer.sharedMaterials;
            if (materialIndex < 0 || materialIndex >= sharedMaterials.Length)
            {
                return;
            }
            original = sharedMaterials[materialIndex];
            instance = Instantiate(original);
            sharedMaterials[materialIndex] = instance;
            renderer.sharedMaterials = sharedMaterials;
        }

        public override void TurnOff()
        {
            KanikamaRuntimeUtility.RemoveBakedEmissiveFlag(instance);
        }

        public override void TurnOn()
        {
            KanikamaRuntimeUtility.AddBakedEmissiveFlag(instance);
            instance.SetColor(propertyName, Color.white);
        }

        public override bool Includes(Object obj) => obj == renderer;

        public override void Clear()
        {
            var sharedMaterials = renderer.sharedMaterials;
            if (materialIndex < 0 || materialIndex >= sharedMaterials.Length)
            {
                return;
            }
            sharedMaterials[materialIndex] = original;
            renderer.sharedMaterials = sharedMaterials;
            DestroyImmediate(instance);
            instance = null;
            original = null;
        }
    }
}
