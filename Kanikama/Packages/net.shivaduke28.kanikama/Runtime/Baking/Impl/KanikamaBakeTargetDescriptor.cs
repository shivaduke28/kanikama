using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.Baking.Impl
{
    public sealed class KanikamaBakeTargetDescriptor : MonoBehaviour, IBakingDescriptor
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
