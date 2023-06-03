using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Utility.Editor
{
    public static class ExtensionMethods
    {
        public static bool IsContributeGI(this GameObject gameObject)
        {
            var flags = GameObjectUtility.GetStaticEditorFlags(gameObject);
            return flags.HasFlag(StaticEditorFlags.ContributeGI);
        }

        public static bool IsEmissiveAndContributeGI(this Renderer renderer)
        {
            return renderer.gameObject.IsContributeGI() &&
                renderer.sharedMaterials.Any(m => m.IsEmissive());
        }
    }
}
