using System.Linq;
using UnityEngine;

namespace Kanikama.Editor.Utility
{
    public static class RendererCollector
    {
        public static Renderer[] CollectKanikamaReceivers()
        {
            return GameObjectUtility.GetComponentsInScene<Renderer>(true)
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
