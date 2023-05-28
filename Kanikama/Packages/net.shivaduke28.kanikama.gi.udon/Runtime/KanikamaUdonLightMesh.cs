using UnityEngine;
using VRC.SDKBase;

namespace Kanikama.GI.Udon
{
    [RequireComponent(typeof(Renderer))]
    public class KanikamaUdonLightMesh : KanikamaUdonLightSource
    {
        [SerializeField] new Renderer renderer;
        [SerializeField] int materialIndex;
        [SerializeField] string propertyName = "_EmissionColor";
        [SerializeField] Material instance;
        [SerializeField] bool useMaterialPropertyBlock = true;

        bool useMaterialPropertyBlockInternal;

        int propertyId;
        MaterialPropertyBlock block;

        void Awake()
        {
            propertyId = VRCShader.PropertyToID(propertyName);
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
    }
}
