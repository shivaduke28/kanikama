using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama
{
    [AddComponentMenu("Kanikama/KanikamaSceneDescriptor")]
    public class KanikamaSceneDescriptor : MonoBehaviour
    {
        [SerializeField] List<KanikamaLight> kanikamaLights;
        [SerializeField] List<KanikamaRendererGroup> kanikamaRendererGroups;
        [SerializeField] List<KanikamaMonitorControl> kanikamaMonitorControls;
        [SerializeField] KanikamaLightSource kanikamaAmbientLight;

        public List<KanikamaLight> KanikamaLights => kanikamaLights;
        public List<KanikamaRendererGroup> KanikamaRendererGroups => kanikamaRendererGroups;
        public List<KanikamaMonitorControl> KanikamaMonitorControls => kanikamaMonitorControls;
        public KanikamaLightSource KanikamaAmbientLight => kanikamaAmbientLight;

        //[SerializeField] List<KanikamaRenderer> kanikamaRenderers;
        //[SerializeField] List<KanikamaLightGroup> kanikamaLightGroups;

        public IReadOnlyList<IKanikamaLightSource> GetLightSources()
        {
            var sources = new List<IKanikamaLightSource>(kanikamaLights);
            sources.Add(kanikamaAmbientLight);
            return sources.AsReadOnly();
        }

        public IReadOnlyList<IKanikamaLightSourceGroup> GetLightSourceGroups()
        {
            var sourceGroups = new List<IKanikamaLightSourceGroup>();
            sourceGroups.AddRange(kanikamaRendererGroups);
            sourceGroups.AddRange(kanikamaMonitorControls);
            return sourceGroups.AsReadOnly();
        }
    }
}