using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Core.Editor
{
    public static class KanikamaEditorUtility
    {
        public static bool IsStaticContributeGI(GameObject gameObject)
        {
            var flags = GameObjectUtility.GetStaticEditorFlags(gameObject);
            return flags.HasFlag(StaticEditorFlags.ContributeGI);
        }

        public static bool IsContributeGI(Renderer renderer)
        {
            return IsStaticContributeGI(renderer.gameObject) &&
                renderer.sharedMaterials.Any(KanikamaRuntimeUtility.IsContributeGI);
        }
    }
}
