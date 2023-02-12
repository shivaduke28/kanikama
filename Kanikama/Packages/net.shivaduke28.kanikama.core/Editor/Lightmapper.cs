using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

namespace Kanikama.Core.Editor
{
    public sealed class Lightmapper
    {
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
