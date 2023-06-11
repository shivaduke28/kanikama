using System.Collections.Generic;

namespace Kanikama.Baking.Experimental.LTC
{
    public interface ILTCDescriptor
    {
        BakeTargetGroup GetMonitorGroup();
        IEnumerable<BakeTargetGroup> GetMonitors();
    }
}
