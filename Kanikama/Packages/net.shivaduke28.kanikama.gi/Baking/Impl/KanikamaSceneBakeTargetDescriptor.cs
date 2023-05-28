using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.GI.Baking.Impl
{
    public sealed class KanikamaSceneBakeTargetDescriptor : MonoBehaviour, IBakingDescriptor
    {
        [SerializeField] List<BakeTarget> bakeTargets;
        [SerializeField] List<BakeTargetGroup> bakeTargetGroups;

        List<BakeTarget> IBakingDescriptor.GetBakeTargets()
        {
            return bakeTargets.ToList();
        }

        List<BakeTargetGroup> IBakingDescriptor.GetBakeTargetGroups()
        {
            return bakeTargetGroups.ToList();
        }
    }
}
