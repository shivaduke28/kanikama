using Kanikama.Utility;
using UnityEngine;

namespace Kanikama
{
    public class KanikamaLightMesh : KanikamaLightSource
    {
        [Header("Baking")] [SerializeField] new Renderer renderer;
        [SerializeField] int materialIndex;
        [SerializeField] string propertyName = "_EmissionColor";

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        [SerializeField] bool useStandardShader = true;
        [SerializeField] string gameObjectTag;

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
#endif
        [Header("Runtime")] [SerializeField] Material instance;
        [SerializeField] bool useMaterialPropertyBlock = true;

        bool useMaterialPropertyBlockInternal;
        int propertyId;
        MaterialPropertyBlock block;

        void Awake()
        {
            propertyId = KanikamaShader.PropertyToID(propertyName);
            useMaterialPropertyBlockInternal = useMaterialPropertyBlock;
            if (useMaterialPropertyBlockInternal)
            {
                block = new MaterialPropertyBlock();
            }
            else
            {
                instance = renderer.materials[materialIndex];
            }
        }

        public override Color GetLinearColor()
        {
            if (useMaterialPropertyBlockInternal)
            {
                renderer.GetPropertyBlock(block);
                return block.GetColor(propertyId).linear;
            }
            else
            {
                return instance.GetColor(propertyId).linear;
            }
        }

        void OnDestroy()
        {
            if (instance != null)
            {
                Destroy(instance);
            }
        }

        void OnValidate()
        {
            if (renderer == null)
            {
                renderer = GetComponent<Renderer>();
            }
        }
    }
}
