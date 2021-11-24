using UnityEditor;
using UnityEngine;

namespace Kanikama.Baking
{
    public class NonKanikamaRenderer
    {
        readonly ObjectReference<Renderer> reference;
        readonly Material[] sharedMaterials;
        readonly Material[] tempMaterials;

        public NonKanikamaRenderer(Renderer renderer, Material[] tempMaterials)
        {
            this.tempMaterials = tempMaterials;
            reference = new ObjectReference<Renderer>(renderer);
            sharedMaterials = renderer.sharedMaterials;
        }

        public static bool IsTarget(Renderer renderer, out NonKanikamaRenderer nonKanikamaRenderer)
        {
            var isTarget = false;
            nonKanikamaRenderer = null;

            var dummyMaterial = new Material(Shader.Find(Baker.ShaderName.Dummy));
            var flag = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);

            if (flag.HasFlag(StaticEditorFlags.ContributeGI))
            {
                var sharedMaterials = renderer.sharedMaterials;
                var count = sharedMaterials.Length;
                var tempMaterials = new Material[count];

                for (var i = 0; i < count; i++)
                {
                    var mat = sharedMaterials[i];
                    if (mat != null && KanikamaLightMaterial.IsTarget(mat))
                    {
                        isTarget = true;
                        tempMaterials[i] = dummyMaterial;
                    }
                    else
                    {
                        tempMaterials[i] = mat;
                    }
                }
                if (isTarget)
                {
                    nonKanikamaRenderer = new NonKanikamaRenderer(renderer, tempMaterials);
                }
            }

            return isTarget;
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