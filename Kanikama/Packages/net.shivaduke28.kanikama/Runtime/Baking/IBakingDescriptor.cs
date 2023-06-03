using System.Collections.Generic;

namespace Kanikama.Baking
{
    public interface IBakingDescriptor
    {
        List<BakeTarget> GetBakeTargets();
        List<BakeTargetGroup> GetBakeTargetGroups();
    }
}
