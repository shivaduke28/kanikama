using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

namespace Kanikama.Editor.Utility
{
    public sealed class UnityLightmapper
    {
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
