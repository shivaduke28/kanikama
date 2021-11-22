using System.Threading;
using System.Threading.Tasks;

namespace Kanikama.Editor
{
    public interface ILightmapper  
    {
        Task BakeAsync(CancellationToken token);
        void Clear();
        bool IsLightMap(string assetPath);
        bool IsDirectionalMap(string assetPath);
        string LightMapDirPath();
    }
}