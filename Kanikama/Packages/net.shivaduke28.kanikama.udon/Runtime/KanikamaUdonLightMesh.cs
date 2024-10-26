using Kanikama.Attributes;
using UnityEngine;
using VRC.SDKBase;

namespace Kanikama.Udon
{
    [RequireComponent(typeof(Renderer))]
    public class KanikamaUdonLightMesh : KanikamaUdonLightSource
    {
        [SerializeField, NonNull] new Renderer renderer;
        [SerializeField] int materialIndex;
        [SerializeField] string propertyName = "_EmissionColor";
        [SerializeField] Material instance;
        [SerializeField] bool useMaterialPropertyBlock = true;

        int propertyId;
        MaterialPropertyBlock block;
        bool initialized;

        void Start()
        {
            propertyId = VRCShader.PropertyToID(propertyName);
            if (useMaterialPropertyBlock)
            {
                block = new MaterialPropertyBlock();
            }
            else
            {
                instance = renderer.materials[materialIndex];
            }
            initialized = true;
        }

        void OnValidate()
        {
            if (renderer == null)
            {
                renderer = GetComponent<Renderer>();
            }
        }

        public override Color GetLinearColor()
        {
            if (!gameObject.activeSelf || !initialized) return Color.black;
            if (useMaterialPropertyBlock)
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
