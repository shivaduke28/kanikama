using System.Collections.Generic;
using System.Linq;
using Kanikama.Core.Attributes;
using UnityEngine;

namespace Kanikama.GI.Baking.Impl
{
    [AddComponentMenu("Kanikama/Baking.KanikamaSceneDescriptor")]
    [EditorOnly]
    public sealed class KanikamaSceneDescriptor : MonoBehaviour, IBakingDescriptor
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
