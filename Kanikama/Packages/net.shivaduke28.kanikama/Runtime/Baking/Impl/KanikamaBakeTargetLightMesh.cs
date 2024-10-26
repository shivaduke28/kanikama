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
        [SerializeField] bool useStandardShader = true;
        [SerializeField] string propertyName = "_EmissionColor";
        [SerializeField] string gameObjectTag;

        void OnValidate()
        {
            renderer = GetComponent<Renderer>();
        }

        public override void Initialize()
        {
            var go = gameObject;
            gameObjectTag = go.tag;
            go.tag = "Untagged";
            if (!TryGetComponent<RendererMaterialHolder>(out var holder))
            {
                holder = go.AddComponent<RendererMaterialHolder>();
            }

            if (useStandardShader)
            {
                var mat = holder.GetMaterial(materialIndex);
                mat.shader = Shader.Find("Standard");
                mat.SetColor(propertyName, Color.white);
                mat.AddBakedEmissiveFlag();
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
            SelectionUtility.SetActiveObject(mat);
        }

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
