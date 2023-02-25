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
        public override IEnumerable<ILightSourceHandle> GetHandles()
        {
            var renderer = GetComponent<Renderer>();

            if (!TryGetComponent<MaterialInstanceHandle>(out var materialInstanceHandle))
            {
                materialInstanceHandle = gameObject.AddComponent<MaterialInstanceHandle>();
            }

            var sharedMaterials = renderer.sharedMaterials;
            var handles = new List<ILightSourceHandle>();
            for (var i = 0; i < sharedMaterials.Length; i++)
            {
                var material = sharedMaterials[i];
                if (MaterialUtility.IsContributeGI(material))
                {
                    handles.Add(new EmissiveMaterialHandle(materialInstanceHandle, i));
                }
            }

            return handles;
        }
    }
}