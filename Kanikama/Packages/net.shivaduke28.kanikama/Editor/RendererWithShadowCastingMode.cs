using UnityEngine;
using UnityEngine.Rendering;

namespace Kanikama.Editor
{
    public sealed class RendererWithShadowCastingMode
    {
        readonly ObjectHandle<Renderer> handle;
        readonly ShadowCastingMode shadowCastingMode;

        public RendererWithShadowCastingMode(Renderer renderer)
        {
            handle = new ObjectHandle<Renderer>(renderer);
            shadowCastingMode = handle.Value.shadowCastingMode;
        }

        public void SetShadowCastingMode(ShadowCastingMode mode)
        {
            handle.Value.shadowCastingMode = mode;
        }

        public void ClearShadowCastingMode()
        {
            handle.Value.shadowCastingMode = shadowCastingMode;
        }
    }
}
