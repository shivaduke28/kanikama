using System.Collections.Generic;
using System.Linq;
using Kanikama.Core;
using UnityEngine;

namespace Kanikama.GI.Implements
{
    [AddComponentMenu("Kanikama/GI/KanikamaSceneDescriptor")]
    [EditorOnly]
    public sealed class KanikamaSceneDescriptor : KanikamaSceneDescriptorBase
    {
        [SerializeField] LightSource[] lightSources;
        [SerializeField] LightSourceGroup[] lightSourceGroups;

        public override ILightSourceHandle[] GetLightSources()
        {
            var list = new List<ILightSourceHandle>();
            list.AddRange(lightSources.Select(x => x.GetHandle()));
            list.AddRange(lightSourceGroups.SelectMany(x => x.GetHandles()));
            return list.ToArray();
        }
    }
}