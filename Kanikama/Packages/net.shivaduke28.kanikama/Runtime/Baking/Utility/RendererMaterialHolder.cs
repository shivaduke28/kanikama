using System.Linq;
using UnityEngine;

namespace Kanikama.Utility
{
    [DisallowMultipleComponent, RequireComponent(typeof(Renderer))]
    public sealed class RendererMaterialHolder : MonoBehaviour
    {
        [SerializeField] new Renderer renderer;
        [SerializeField] Material[] sharedMaterials;
        [SerializeField] Material[] materials;
        [SerializeField] bool initialized;

        void OnValidate() => Initialize();
        void Awake() => Initialize();
        void OnDestroy() => Clear();
        public void Reset() => Clear();

        void Initialize()
        {
            if (initialized) return;
            renderer = GetComponent<Renderer>();
            sharedMaterials = renderer.sharedMaterials;
            materials = sharedMaterials.Select(Instantiate).ToArray();
            renderer.sharedMaterials = materials;
            initialized = true;
        }

        public Material[] GetMaterials() => materials;
        public Material GetMaterial(int index) => materials[index];

        public void Clear()
        {
            if (!initialized) return;
            if (sharedMaterials != null)
            {
                renderer.sharedMaterials = sharedMaterials;
                sharedMaterials = null;
            }
            if (materials != null)
            {
                foreach (var material in materials)
                {
                    material.DestroySafely();
                }
                materials = null;
            }
            initialized = false;
        }
    }
}
