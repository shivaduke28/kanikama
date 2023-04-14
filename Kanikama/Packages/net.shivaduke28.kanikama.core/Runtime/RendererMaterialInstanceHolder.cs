using System;
using System.Linq;
using UnityEngine;

namespace Kanikama.Core
{
    [DisallowMultipleComponent, RequireComponent(typeof(Renderer))]
    public sealed class RendererMaterialInstanceHolder : MonoBehaviour, IDisposable
    {
        [SerializeField] new Renderer renderer;
        [SerializeField] Material[] sharedMaterials;
        [SerializeField] Material[] materials;
        [SerializeField] bool initialized;

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
            Initialize();
            return materials;
        }

        public void Dispose()
        {
            if (!initialized) return;
            renderer.sharedMaterials = sharedMaterials;
            if (materials != null)
            {
                foreach (var material in materials)
                {
                    KanikamaRuntimeUtility.DestroySafe(material);
                }
            }
        }
    }
}
