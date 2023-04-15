using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core.Editor;
using Kanikama.GI.Baking;
using Kanikama.GI.Editor;

namespace Kanikama.GI.Bakery.Editor
{
    public static class BakeryBakingPipelineRunner
    {
        public static async Task RunAsync(IBakingDescriptor bakingDescriptor, BakeryBakingSetting bakingSetting, CancellationToken cancellationToken)
        {
            var sceneAssetData = KanikamaSceneUtility.ToAssetData(bakingSetting.SceneAsset);
            var settingAsset = BakeryBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var bakeTargets = bakingDescriptor.GetBakeTargets();
            var handles = bakeTargets.Select(x => new BakeTargetHandle<BakeTarget>(x)).Cast<IBakeTargetHandle>().ToList();
            var groups = bakingDescriptor.GetBakeTargetGroups();
            foreach (var group in groups)
            {
                var list = group.GetAll();
                for (var i = 0; i < list.Count; i++)
                {
                    handles.Add(new BakeTargetGroupElementHandle<BakeTargetGroup>(group, i));
                }
            }

            var ctx = new BakeryBakingPipeline.Context(
                sceneAssetData,
                handles,
                new BakeryLightmapper(),
                settingAsset.Setting
            );

            await BakeryBakingPipeline.BakeAsync(ctx, default);
        }

        public static void CreateAssets(BakeryBakingSetting setting)
        {
            BakeryBakingPipeline.CreateAssets(setting);
        }
    }
}
