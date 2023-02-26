using System;
using Kanikama.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Implements
{
    public sealed class EmissiveMaterialHandle : ILightSourceHandle
    {
        readonly ComponentReference<Renderer> rendererReference;
        readonly int index;
        ComponentReference<MaterialInstanceHandle> materialReference;

        public EmissiveMaterialHandle(Renderer renderer, int index)
        {
            this.rendererReference = new ComponentReference<Renderer>(renderer);
            this.index = index;
        }

        // with side effect to the active scene.
        public void Initialize()
        {
            var renderer = rendererReference.Value;
            if (!renderer.gameObject.TryGetComponent<MaterialInstanceHandle>(out var materialInstanceHandle))
            {
                materialInstanceHandle = renderer.gameObject.AddComponent<MaterialInstanceHandle>();
            }

            materialReference = new ComponentReference<MaterialInstanceHandle>(materialInstanceHandle);
            materialReference.Value.CreateInstances();
        }

        public void TurnOn()
        {
            MaterialUtility.AddBakedEmissiveFlag(materialReference.Value.GetInstance(index));
        }

        public void TurnOff()
        {
            MaterialUtility.RemoveBakedEmissiveFlag(materialReference.Value.GetInstance(index));
        }

        public bool Includes(Object obj) => obj is Renderer renderer && renderer == rendererReference.Value;

        void IDisposable.Dispose()
        {
            var instanceHandle = materialReference.Value;
            if (instanceHandle != null)
            {
                instanceHandle.Clear();
                Object.DestroyImmediate(instanceHandle);
            }
        }
    }
}