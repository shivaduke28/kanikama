using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama
{
    [AddComponentMenu("Kanikama/KanikamaSceneDescriptor")]
    public class KanikamaSceneDescriptor : MonoBehaviour
    {
        [SerializeField] List<KanikamaLight> kanikamaLights;
        [SerializeField] List<KanikamaRenderer> kanikamaRenderers;
        [SerializeField] List<KanikamaLightSourceGroup> kanikamaLightSourceGroups;

        public List<KanikamaLight> KanikamaLights => kanikamaLights;
        public List<KanikamaRenderer> KanikamaRendererGroups => kanikamaRenderers;
        public List<KanikamaLightSourceGroup> KanikamaLightSourceGroups => kanikamaLightSourceGroups;

        public IReadOnlyList<KanikamaLightSource> GetLightSources()
        {
            var sources = new List<KanikamaLightSource>();
            sources.AddRange(kanikamaLights);
            return sources.AsReadOnly();
        }

        public IReadOnlyList<KanikamaLightSourceGroup> GetLightSourceGroups()
        {
            var sourceGroups = new List<KanikamaLightSourceGroup>();
            sourceGroups.AddRange(kanikamaRenderers);
            sourceGroups.AddRange(kanikamaLightSourceGroups);
            return sourceGroups.AsReadOnly();
        }
    }
}