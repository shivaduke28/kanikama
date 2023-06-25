using Kanikama.Baking.Attributes;
using Kanikama.Utility;
using UnityEngine;

namespace Kanikama.Baking.Impl
{
    [RequireComponent(typeof(Renderer))]
    public sealed class KanikamaBakeTargetLightMesh : BakeTarget
    {
        [SerializeField, NonNull] new Renderer renderer;
        [SerializeField] int materialIndex;
        [SerializeField] string propertyName = "_EmissionColor";
        [SerializeField, HideInInspector] string gameObjectTag;

        void OnValidate()
        {
            renderer = GetComponent<Renderer>();
        }

        public override void Initialize()
        {
            var go = gameObject;
            gameObjectTag = go.tag;
            go.tag = "Untagged";
            if (!TryGetComponent<RendererMaterialHolder>(out _))
            {
                go.AddComponent<RendererMaterialHolder>();
            }
        }

        public override void TurnOff()
        {
            gameObject.tag = gameObjectTag;
            var holder = GetComponent<RendererMaterialHolder>();
            holder.GetMaterial(materialIndex).RemoveBakedEmissiveFlag();
        }

        public override void TurnOn()
        {
            var holder = GetComponent<RendererMaterialHolder>();
            var mat = holder.GetMaterial(materialIndex);
            mat.SetColor(propertyName, Color.white);
            mat.AddBakedEmissiveFlag();
        }

        public override bool Includes(Object obj) => obj == renderer;

        public override void Clear()
        {
            gameObject.tag = gameObjectTag;
            if (TryGetComponent<RendererMaterialHolder>(out var holder))
            {
                holder.Clear();
                holder.DestroySafely();
            }
        }
    }
}
