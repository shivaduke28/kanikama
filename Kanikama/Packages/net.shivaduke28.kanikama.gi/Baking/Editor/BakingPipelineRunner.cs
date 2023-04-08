using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core.Editor;
using Kanikama.GI.Baking;

namespace Kanikama.GI.Editor
{
    public class BakingPipelineRunner
    {
        public static async Task RunAsync(IBakingDescriptor bakingDescriptor, BakingConfiguration bakingConfiguration, SceneAssetData sceneAssetData,
            CancellationToken cancellationToken)
        {
            // GetBakeables() が null を返さないためには、アクティブシーンのObjectを参照している必要がありそう
            var bakeables = bakingDescriptor.GetBakeTargets();
            var handles = bakeables.Select(x => new BakeTargetHandle<BakeTarget>(x)).Cast<IBakeTargetHandle>().ToList();
            var groups = bakingDescriptor.GetBakeTargetGroups();
            foreach (var group in groups)
            {
                var list = group.GetAll();
                for (var i = 0; i < list.Count; i++)
                {
                    handles.Add(new BakeTargetGroupElementHandle<BakeTargetGroup>(group, i));
                }
            }
            var context = new BakingPipeline.BakingContext
            {
                BakingConfiguration = bakingConfiguration,
                BakeableHandles = handles,
                SceneAssetData = sceneAssetData,
            };

            await BakingPipeline.BakeAsync(context, cancellationToken);
        }

        public static async Task RunWithoutKanikamaAsync(IBakingDescriptor bakingDescriptor, BakingConfiguration bakingConfiguration,
            SceneAssetData sceneAssetData,
            CancellationToken cancellationToken)
        {
            // GetBakeables() が null を返さないためには、アクティブシーンのObjectを参照している必要がありそう
            var bakeables = bakingDescriptor.GetBakeTargets();
            var handles = bakeables.Select(x => new BakeTargetHandle<BakeTarget>(x)).Cast<IBakeTargetHandle>().ToList();
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
