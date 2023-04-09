using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

namespace Kanikama.Core.Editor
{
    public interface ILightmapper
    {
        void ClearCache();
        Task BakeAsync(CancellationToken cancellationToken);
    }

    [InitializeOnLoad]
    public sealed class Lightmapper : ILightmapper
    {
        static ILightmapper Create() => new Lightmapper();

        static Lightmapper()
        {
            LightmapperFactory.Register("Unity", Create, 0);
        }

        public void ClearCache()
        {
            Lightmapping.ClearDiskCache();
        }

        public async Task BakeAsync(CancellationToken cancellationToken)
        {
            if (!Lightmapping.BakeAsync())
            {
                throw new OperationCanceledException("Lightmapping.BakeAsync() failed.");
            }

            while (Lightmapping.isRunning)
            {
                try
                {
                    await Task.Delay(33, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    Lightmapping.Cancel();
                    throw;
                }
            }
        }
    }
}
