using System.Collections.Generic;

namespace Kanikama.Baking.LTC
{
    public interface ILTCDescriptor
    {
        BakeTargetGroup GetMonitorGroup();
        IEnumerable<BakeTargetGroup> GetMonitors();
    }
}
