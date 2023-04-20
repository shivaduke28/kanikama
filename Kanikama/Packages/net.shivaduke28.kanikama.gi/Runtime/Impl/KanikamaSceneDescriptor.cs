using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.GI.Runtime.Impl
{
    [AddComponentMenu("Kanikama/Runtime.KanikamaSceneDescriptor")]
    public sealed class KanikamaSceneDescriptor : MonoBehaviour
    {
        [SerializeField] List<LightSource> lightSources;
        [SerializeField] List<LightSourceGroup> lightSourceGroups;

        public List<ILightSource> GetLightSources() => lightSources.Cast<ILightSource>().ToList();
        public List<ILightSourceGroup> GetLightSourceGroups => lightSourceGroups.Cast<ILightSourceGroup>().ToList();
    }
}
