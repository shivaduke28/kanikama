using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core.Editor;

namespace Kanikama.GI.Editor
{
    public class BakingPipelineRunner
    {
        public static async Task RunAsync(IBakingDescriptor bakingDescriptor, BakingConfiguration bakingConfiguration, SceneAssetData sceneAssetData,
            CancellationToken cancellationToken)
        {
            // GetBakeables() が null を返さないためには、アクティブシーンのObjectを参照している必要がありそう
            var bakeables = bakingDescriptor.GetBakeables();
            var handles = bakeables.Select(x => new BakeableHandle<Bakeable>(x)).Cast<IBakeableHandle>().ToList();
            var context = new BakingPipeline.BakingContext
            {
                BakingConfiguration = bakingConfiguration,
                BakeableHandles = handles,
                SceneAssetData = sceneAssetData,
            };

            await BakingPipeline.BakeAsync(context, cancellationToken);
        }
        
        public static async Task RunWithoutKanikamaAsync(IBakingDescriptor bakingDescriptor, BakingConfiguration bakingConfiguration, SceneAssetData sceneAssetData,
            CancellationToken cancellationToken)
        {
            // GetBakeables() が null を返さないためには、アクティブシーンのObjectを参照している必要がありそう
            var bakeables = bakingDescriptor.GetBakeables();
            var handles = bakeables.Select(x => new BakeableHandle<Bakeable>(x)).Cast<IBakeableHandle>().ToList();
            var context = new BakingPipeline.BakingContext
            {
                BakingConfiguration = bakingConfiguration,
                BakeableHandles = handles,
                SceneAssetData = sceneAssetData,
            };

            await BakingPipeline.BakeWithoutKanikamaAsync(context, cancellationToken);
        }
    }
}
