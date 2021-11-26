using System.Collections.Generic;
using UnityEngine;

namespace Kanikama
{
    public class KanikamaSceneDescriptor : MonoBehaviour
    {
        [SerializeField] List<KanikamaLight> kanikamaLights;
        [SerializeField] List<KanikamaRenderer> kanikamaRenderers;
        [SerializeField] List<KanikamaLightSourceGroup> kanikamaLightSourceGroups;

        public List<KanikamaLight> KanikamaLights => kanikamaLights;
        public List<KanikamaRenderer> KanikamaRendererGroups => kanikamaRenderers;
        public List<KanikamaLightSourceGroup> KanikamaLightSourceGroups => kanikamaLightSourceGroups;

        public IReadOnlyList<LightSource> GetLightSources()
        {
            var sources = new List<LightSource>();
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