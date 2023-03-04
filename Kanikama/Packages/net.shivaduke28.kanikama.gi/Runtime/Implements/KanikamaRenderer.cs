using System.Collections.Generic;
using Kanikama.Core;
using UnityEngine;

namespace Kanikama.GI.Implements
{
    [RequireComponent(typeof(Renderer))]
    [AddComponentMenu("Kanikama/GI/KanikamaRenderer")]
    [EditorOnly]
    public sealed class KanikamaRenderer : LightSourceGroup
    {
        [SerializeField] int[] materialIndices = { 0 };

        public override IEnumerable<ILightSourceHandle> GetHandles()
        {
            var rend = GetComponent<Renderer>();
            var sharedMaterials = rend.sharedMaterials;
            var handles = new List<ILightSourceHandle>();
            foreach (var materialIndex in materialIndices)
            {
                if (materialIndex < 0 || sharedMaterials.Length <= materialIndex)
                {
                    KanikamaDebug.LogError($"Invalid material index {materialIndex}.");
                    continue;
                }
                var material = sharedMaterials[materialIndex];
                if (MaterialUtility.IsContributeGI(material))
                {
                    handles.Add(new EmissiveMaterialHandle(rend, materialIndex));
                }
                else
                {
                    KanikamaDebug.LogError($"Material with index {materialIndex} will not contribute to GI.");
                }
            }
            return handles;
        }
    }
}
