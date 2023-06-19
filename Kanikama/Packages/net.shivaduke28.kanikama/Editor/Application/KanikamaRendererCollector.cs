using System.Linq;
using Kanikama.Editor.Baking;
using UnityEngine;

namespace Editor.Application
{
    public static class RendererCollector
    {
        public static Renderer[] CollectKanikamaReceivers()
        {
            return GameObjectHelper.GetComponentsInScene<Renderer>(true)
                .Where(r => r.gameObject.IsContributeGI())
                .Where(IsUsingKanikamaShader)
                .ToArray();
        }

        static bool IsUsingKanikamaShader(Renderer renderer)
        {
            return renderer.sharedMaterials.Any(m => m.GetTag("KanikamaGI", true) == "true");
        }
    }
}
