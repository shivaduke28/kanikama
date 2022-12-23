using System.Threading;
using System.Threading.Tasks;

namespace Kanikama.Baking
{
    public interface ILightmapper  
    {
        Task BakeAsync(CancellationToken token);
        void Clear();
        bool IsDirectionalMode();
    }
}