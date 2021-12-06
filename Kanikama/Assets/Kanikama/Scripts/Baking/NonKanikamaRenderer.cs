using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Baking
{
    public class NonKanikamaRenderer
    {
        readonly ObjectReference<Renderer> reference;
        readonly Material[] sharedMaterials;
        readonly Material[] tempMaterials;

        public NonKanikamaRenderer(Renderer renderer)
        {
            reference = new ObjectReference<Renderer>(renderer);
            sharedMaterials = renderer.sharedMaterials;
            var temp = sharedMaterials.Select(x => Object.Instantiate(x));
            foreach(var mat in temp)
            {
                KanikamaLightMaterial.RemoveBakedEmissiveFlag(mat);
            }
            tempMaterials = temp.ToArray();
        }

        public static bool IsTarget(Renderer renderer)
        {
            var flags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
            if (!flags.HasFlag(StaticEditorFlags.ContributeGI)) return false;
            return renderer.sharedMaterials.Any(m => KanikamaLightMaterial.IsBakedEmissive(m));
        }

        public void TurnOff()
        {
            reference.Value.sharedMaterials = tempMaterials;
        }

        public void Rollback()
        {
            reference.Value.sharedMaterials = sharedMaterials;
            foreach (var mat in tempMaterials)
            {
                if (mat != null) Object.DestroyImmediate(mat);
            }
        }
    }
}