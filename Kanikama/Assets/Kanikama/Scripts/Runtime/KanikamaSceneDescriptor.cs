using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama
{
    [AddComponentMenu("Kanikama/KanikamaSceneDescriptor")]
    public class KanikamaSceneDescriptor : MonoBehaviour
    {
        [SerializeField] KanikamaLight kanikamaAmbientLight;
        [SerializeField] List<KanikamaLight> kanikamaLights;
        [SerializeField] List<KanikamaRenderer> kanikamaRenderers;
        [SerializeField] List<KanikamaLightSourceGroup> kanikamaLightSourceGroups;

        public List<KanikamaLight> KanikamaLights => kanikamaLights;
        public List<KanikamaRenderer> KanikamaRendererGroups => kanikamaRenderers;
        public List<KanikamaLightSourceGroup> KanikamaLightSourceGroups => kanikamaLightSourceGroups;
        public KanikamaLight KanikamaAmbientLight => kanikamaAmbientLight;

        public IReadOnlyList<IKanikamaLightSource> GetLightSources()
        {
            var sources = new List<IKanikamaLightSource>() { kanikamaAmbientLight };
            sources.AddRange(kanikamaLights);
            return sources.AsReadOnly();
        }

        public IReadOnlyList<IKanikamaLightSourceGroup> GetLightSourceGroups()
        {
            var sourceGroups = new List<IKanikamaLightSourceGroup>();
            sourceGroups.AddRange(kanikamaRenderers);
            sourceGroups.AddRange(kanikamaLightSourceGroups);
            return sourceGroups.AsReadOnly();
        }
    }
}