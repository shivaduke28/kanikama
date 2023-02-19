using System;
using Kanikama.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Implements
{
    public sealed class LightHandle : ILightSourceHandle
    {
        readonly ComponentReference<Light> reference;

        public LightHandle(Light light)
        {
            reference = new ComponentReference<Light>(light);
        }

        public void Initialize()
        {
        }

        public void TurnOn()
        {
            reference.Value.color = Color.white;
            reference.Value.intensity = 1f;
        }

        public void TurnOff()
        {
            reference.Value.intensity = 0f;
        }

        public bool Includes(Object obj) => obj is Light light && reference.Value == light;

        void IDisposable.Dispose()
        {
        }
    }
}
