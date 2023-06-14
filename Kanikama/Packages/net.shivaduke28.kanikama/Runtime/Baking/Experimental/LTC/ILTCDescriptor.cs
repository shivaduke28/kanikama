using System.Collections.Generic;

namespace Kanikama.Baking.Experimental.LTC
{
    public interface ILTCDescriptor
    {
        IEnumerable<LTCMonitor> GetMonitors();
    }
}
