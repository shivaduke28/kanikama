using System;
using System.Linq;
using UnityEngine;

namespace Kanikama.Core
{
    [RequireComponent(typeof(Renderer))]
    [EditorOnly]
    public sealed class MaterialInstanceHandle : MonoBehaviour
    {
        [SerializeField] Renderer renderer;
        [SerializeField] Material[] sharedMaterials;
        [SerializeField] Material[] instances;
        [SerializeField] bool isInstantiated;
        public Renderer Renderer => renderer != null ? renderer : renderer = GetComponent<Renderer>();

        public void CreateInstances()
        {
            if (isInstantiated) return;

            sharedMaterials = Renderer.sharedMaterials;
            instances = sharedMaterials.Select(Instantiate).ToArray();
            renderer.sharedMaterials = instances;
            isInstantiated = true;
        }

        public Material GetInstance(int index)
        {
            if (!isInstantiated)
            {
                throw new Exception("CreateInstances() should be called before try to get instances.");
            }
            return instances[index];
        }

        public void Clear()
        {
            if (!isInstantiated) return;

            renderer.sharedMaterials = sharedMaterials;
            foreach (var instance in instances)
            {
                DestroyImmediate(instance);
            }

            instances = null;
            isInstantiated = false;
        }
    }
}