using System.Threading;
using System.Threading.Tasks;

namespace Kanikama.Baking
{
    public interface ILightmapper  
    {
        Task BakeAsync(CancellationToken token);
        void Clear();
        bool IsLightmap(string assetPath);
        bool IsDirectionalMap(string assetPath);
        bool IsDirectionalMode();
        string LightmapDirPath();
    }
}