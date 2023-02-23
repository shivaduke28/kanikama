using System;
using Kanikama.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Implements
{
    public sealed class EmissiveMaterialHandle : ILightSourceHandle
    {
        readonly ComponentReference<MaterialInstanceHandle> reference;
        readonly int index;

        public EmissiveMaterialHandle(MaterialInstanceHandle renderer, int index)
        {
            reference = new ComponentReference<MaterialInstanceHandle>(renderer);
            this.index = index;
        }

        // with side effect to the active scene.
        public void Initialize()
        {
            reference.Value.CreateInstances();
        }

        public void TurnOn()
        {
            MaterialUtility.AddBakedEmissiveFlag(reference.Value.GetInstance(index));
        }

        public void TurnOff()
        {
            MaterialUtility.RemoveBakedEmissiveFlag(reference.Value.GetInstance(index));
        }

        public bool Includes(Object obj) => obj is Renderer renderer && renderer == reference.Value.Renderer;

        void IDisposable.Dispose()
        {
            reference.Value.Clear();
        }
    }
}