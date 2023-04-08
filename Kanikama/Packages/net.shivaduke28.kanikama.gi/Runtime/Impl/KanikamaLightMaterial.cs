using UnityEngine;

namespace Kanikama.GI.Runtime.Impl
{
    [RequireComponent(typeof(Renderer))]
    [AddComponentMenu("Kanikama/GI/Runtime.KanikamaLightMaterial")]
    public sealed class KanikamaLightMaterial : LightSource
    {
        [SerializeField] new Renderer renderer;
        [SerializeField] int materialIndex;
        [SerializeField] string propertyName = "_EmissionColor";
        [SerializeField] Material instance;
        [SerializeField] bool useMaterialPropertyBlock = true;

        bool useMaterialPropertyBlockInternal;

        int propertyId;
        MaterialPropertyBlock block;

        void Start()
        {
            propertyId = Shader.PropertyToID(propertyName);
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

        void OnValidate()
        {
            renderer = GetComponent<Renderer>();
        }

        public override Color GetColorLinear()
        {
            // Because we assume emissive colors are HDR,
            // so that color values are already linear.
            if (useMaterialPropertyBlockInternal)
            {
                renderer.GetPropertyBlock(block);
                return block.GetColor(propertyId);
            }
            else
            {
                return instance.GetColor(propertyId);
            }
        }

        void OnDestroy()
        {
            if (instance != null)
            {
                Destroy(instance);
            }
        }
    }
}
