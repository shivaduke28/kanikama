using System.Collections.Generic;
using Kanikama.Core;
using UnityEngine;

namespace Kanikama.GI.Implements
{
    [RequireComponent(typeof(Renderer))]
    public sealed class KanikamaRenderer : LightSourceGroup
    {
        public override IEnumerable<ILightSourceHandle> GetHandles()
        {
            var renderer = GetComponent<Renderer>();
            var sharedMaterials = renderer.sharedMaterials;
            var handles = new List<ILightSourceHandle>();
            for (var i = 0; i < sharedMaterials.Length; i++)
            {
                var material = sharedMaterials[i];
                if (MaterialUtility.IsContributeGI(material))
                {
                    handles.Add(new EmissiveMaterialHandle(renderer, i));
                }
            }
            return handles;
        }
    }
}
