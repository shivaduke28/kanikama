using System;
using Kanikama.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Implements
{
    public sealed class EmissiveMaterialHandle : ILightSourceHandle
    {
        readonly ComponentReference<Renderer> reference;
        readonly int index;
        Material material;
        Material sharedMaterial;

        public EmissiveMaterialHandle(Renderer renderer, int index)
        {
            reference = new ComponentReference<Renderer>(renderer);
            this.index = index;
        }

        // with side effect to the active scene.
        public void Initialize()
        {
            var renderer = reference.Value;
            var sharedMaterials = renderer.sharedMaterials;
            sharedMaterial = sharedMaterials[index];
            material = Object.Instantiate(sharedMaterial);
            sharedMaterials[index] = material;
            renderer.sharedMaterials = sharedMaterials;
        }

        public void TurnOn()
        {
            MaterialUtility.AddBakedEmissiveFlag(material);
        }

        public void TurnOff()
        {
            MaterialUtility.RemoveBakedEmissiveFlag(material);
        }

        public bool Includes(Object obj) => obj is Renderer renderer && renderer == reference.Value;

        void IDisposable.Dispose()
        {
            Object.DestroyImmediate(material);
            var renderer = reference.Value;
            var sharedMaterials = renderer.sharedMaterials;
            sharedMaterials[index] = sharedMaterial;
            renderer.sharedMaterials = sharedMaterials;
        }
    }
}
