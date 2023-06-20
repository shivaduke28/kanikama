using System.Collections.Generic;
using System.Linq;
using Kanikama.Baking.Impl.LTC;
using UnityEngine;

namespace Kanikama.Baking.Impl
{
    public sealed class KanikamaBakeTargetDescriptor : MonoBehaviour
    {
        [SerializeField] List<BakeTarget> bakeTargets;
        [SerializeField] List<BakeTargetGroup> bakeTargetGroups;
        [SerializeField] List<KanikamaLTCMonitor> ltcMonitors;

        public List<BakeTarget> GetBakeTargets() => bakeTargets.ToList();

        public List<BakeTargetGroup> GetBakeTargetGroups() => bakeTargetGroups.ToList();
        public KanikamaLTCMonitor[] GetLTCMonitors() => ltcMonitors.ToArray();
    }
}
