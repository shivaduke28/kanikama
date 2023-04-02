using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.GI.Implements
{
    [AddComponentMenu("Kanikama/GI/KanikamaSceneDescriptor")]
    public sealed class KanikamaSceneDescriptor : MonoBehaviour, IBakingDescriptor
    {
        [SerializeField] List<LightSource> lightSources;

        public List<ILightSource> GetLightSources() => lightSources.Cast<ILightSource>().ToList();

        List<Bakeable> IBakingDescriptor.GetBakeables() => lightSources.Cast<Bakeable>().ToList();
    }
}
