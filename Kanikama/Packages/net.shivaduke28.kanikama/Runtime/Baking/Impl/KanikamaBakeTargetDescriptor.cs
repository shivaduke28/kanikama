using System.Collections.Generic;
using System.Linq;
using Kanikama.Baking.Attributes;
using Kanikama.Baking.Impl.LTC;
using UnityEngine;

namespace Kanikama.Baking.Impl
{
    public sealed class KanikamaBakeTargetDescriptor : MonoBehaviour
    {
        [SerializeField, NonNull] List<BakeTarget> bakeTargets = new List<BakeTarget>();
        [SerializeField, NonNull] List<BakeTargetGroup> bakeTargetGroups = new List<BakeTargetGroup>();
        [SerializeField, NonNull] List<LTCMonitor> ltcMonitors = new List<LTCMonitor>();

        public List<BakeTarget> GetBakeTargets() => bakeTargets.ToList();

        public List<BakeTargetGroup> GetBakeTargetGroups() => bakeTargetGroups.ToList();
        public LTCMonitor[] GetLTCMonitors() => ltcMonitors.ToArray();

        public bool Validate()
        {
            return bakeTargets.All(x => x != null)
                && bakeTargetGroups.All(x => x != null)
                && ltcMonitors.All(x => x != null);
        }
    }
}
