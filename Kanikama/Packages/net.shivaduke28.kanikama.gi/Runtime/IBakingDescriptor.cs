using System.Collections.Generic;

namespace Kanikama.GI
{
    public interface IBakingDescriptor
    {
        List<Bakeable> GetBakeables();
    }
}
