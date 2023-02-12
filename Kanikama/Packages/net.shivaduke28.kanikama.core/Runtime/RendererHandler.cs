using System.Linq;
using UnityEngine;

namespace Kanikama.Core
{
    public sealed class RendererHandler
    {
        readonly Renderer renderer;

        public RendererHandler(Renderer renderer)
        {
            this.renderer = renderer;
        }

        public void TurnOff()
        {
        }

        public void Revert()
        {
        }
    }
}
