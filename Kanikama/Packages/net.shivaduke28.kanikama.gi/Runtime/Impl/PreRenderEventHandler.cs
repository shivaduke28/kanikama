using System;
using UnityEngine;

namespace Kanikama.GI.Runtime.Impl
{
    [RequireComponent(typeof(Camera))]
    public sealed class PreRenderEventHandler : MonoBehaviour
    {
        public event Action OnPreRenderEvent;

        void OnPreRender()
        {
            OnPreRenderEvent?.Invoke();
        }
    }
}
