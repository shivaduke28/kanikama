using System.Collections.Generic;

namespace Kanikama.GI.Baking
{
    public interface IBakingDescriptor
    {
        List<BakeTarget> GetBakeTargets();
        List<BakeTargetGroup> GetBakeTargetGroups();
    }
}
