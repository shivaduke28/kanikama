using Kanikama.Core;
using UnityEngine;

namespace Kanikama.GI.Baking.Impl
{
    [RequireComponent(typeof(Renderer))]
    [AddComponentMenu("Kanikama/Baking.KanikamaLightMaterial")]
    public sealed class KanikamaLightMaterial : BakeTarget
    {
        [SerializeField] new Renderer renderer;
        [SerializeField] int materialIndex;
        [SerializeField] string propertyName = "_EmissionColor";
        [SerializeField] string tag;

        void OnValidate()
        {
            renderer = GetComponent<Renderer>();
        }

        public override void Initialize()
        {
            gameObject.SetActive(true);
            renderer.enabled = true;
            tag = gameObject.tag;
            if (!TryGetComponent<RendererMaterialHolder>(out _))
            {
                gameObject.AddComponent<RendererMaterialHolder>();
            }
        }

        public override void TurnOff()
        {
            gameObject.tag = tag;
            var holder = GetComponent<RendererMaterialHolder>();
            holder.GetMaterial(materialIndex).RemoveBakedEmissiveFlag();
        }

        public override void TurnOn()
        {
            gameObject.tag = "Untagged";
            var holder = GetComponent<RendererMaterialHolder>();
            var mat = holder.GetMaterial(materialIndex);
            mat.SetColor(propertyName, Color.white);
            mat.AddBakedEmissiveFlag();
        }

        public override bool Includes(Object obj) => obj == renderer;

        public override void Clear()
        {
            gameObject.tag = tag;
            if (TryGetComponent<RendererMaterialHolder>(out var holder))
            {
                holder.Clear();
                holder.DestroySafely();
            }
        }
    }
}
