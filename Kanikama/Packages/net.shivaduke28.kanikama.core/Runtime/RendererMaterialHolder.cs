using System;
using System.Linq;
using UnityEngine;

namespace Kanikama.Core
{
    [DisallowMultipleComponent, RequireComponent(typeof(Renderer))]
    public sealed class RendererMaterialHolder : MonoBehaviour
    {
        [SerializeField] new Renderer renderer;
        [SerializeField] Material[] sharedMaterials;
        [SerializeField] Material[] materials;
        [SerializeField] bool initialized;

        void OnValidate()
        {
            Initialize();
        }

        void Awake()
        {
            Initialize();
        }

        void Initialize()
        {
            if (initialized) return;
            renderer = GetComponent<Renderer>();
            sharedMaterials = renderer.sharedMaterials;
            materials = sharedMaterials.Select(Instantiate).ToArray();
            renderer.sharedMaterials = materials;
            initialized = true;
        }

        public Material[] GetMaterials()
        {
            return materials;
        }

        public Material GetMaterial(int index)
        {
            return materials[index];
        }

        public void Clear()
        {
            if (sharedMaterials != null)
            {
                renderer.sharedMaterials = sharedMaterials;
            }
            if (materials != null)
            {
                foreach (var material in materials)
                {
                    KanikamaRuntimeUtility.DestroySafe(material);
                }
            }
            initialized = false;
        }
    }
}
