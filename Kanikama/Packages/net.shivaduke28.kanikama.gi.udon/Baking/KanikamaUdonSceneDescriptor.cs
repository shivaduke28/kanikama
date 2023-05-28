using System.Collections.Generic;
using System.Linq;
using Kanikama.GI.Baking;
using Kanikama.GI.Baking.Impl;
using UnityEngine;

namespace Kanikama.GI.Udon.Baking
{
    public sealed class KanikamaUdonSceneDescriptor : MonoBehaviour, IBakingDescriptor
    {
        [SerializeField] KanikamaBakeTargetLight[] kanikamaBakeTargetLights;
        [SerializeField] KanikamaBakeTargetLightMesh[] kanikamaBakeTargetLightMeshes;
        [SerializeField] KanikamaBakeTargetMonitorGroup[] kanikamaBakeTargetMonitorGroups;

        // Used only to setup ColorCollector in Editor
        public Light[] GetLights() => kanikamaBakeTargetLights.Select(x => x.GetComponent<Light>()).ToArray();

        public Renderer[] GetRenderers() => kanikamaBakeTargetLights.Select(x => x.GetComponent<Renderer>()).ToArray();

        List<BakeTarget> IBakingDescriptor.GetBakeTargets()
        {
            var targets = new List<BakeTarget>(kanikamaBakeTargetLights);
            targets.AddRange(kanikamaBakeTargetLightMeshes);
            return targets;
        }

        List<BakeTargetGroup> IBakingDescriptor.GetBakeTargetGroups()
        {
            return kanikamaBakeTargetMonitorGroups.Cast<BakeTargetGroup>().ToList();
        }
    }
}
