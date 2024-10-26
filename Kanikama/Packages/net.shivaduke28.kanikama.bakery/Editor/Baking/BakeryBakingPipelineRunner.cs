using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Editor;
using Kanikama.Editor.Baking;
using Kanikama.Impl;

namespace Kanikama.Bakery.Editor.Baking
{
    public static class BakeryBakingPipelineRunner
    {
        public static async Task BakeAsync(KanikamaBakeTargetDescriptor bakingDescriptor, BakeryBakingSetting bakingSetting,
            CancellationToken cancellationToken)
        {
            var sceneAssetData = new SceneAssetData(bakingSetting.SceneAsset);
            var settingAsset = BakeryBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var commands = CreateCommands(bakingDescriptor);

            var ctx = new BakeryBakingPipeline.Parameter(
                sceneAssetData,
                settingAsset.Setting,
                commands
            );

            await BakeryBakingPipeline.BakeAsync(ctx, cancellationToken);
        }

        public static async Task BakeStaticAsync(KanikamaBakeTargetDescriptor bakingDescriptor,
            SceneAssetData sceneAssetData,
            CancellationToken cancellationToken)
        {
            var commands = CreateCommands(bakingDescriptor);
            var settingAsset = BakeryBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var context = new BakeryBakingPipeline.Parameter(sceneAssetData, settingAsset.Setting, commands);

            await BakeryBakingPipeline.BakeStaticAsync(context, cancellationToken);
        }

        public static void CreateAssets(KanikamaBakeTargetDescriptor bakingDescriptor, SceneAssetData sceneAssetData)
        {
            var handles = CreateHandles(bakingDescriptor);
            var settingAsset = BakeryBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var setting = settingAsset.Setting;
            BakeryBakingPipeline.CreateAssets(handles, setting);
        }

        static List<IBakeTargetHandle> CreateHandles(KanikamaBakeTargetDescriptor bakingDescriptor)
        {
            var bakeTargets = bakingDescriptor.GetBakeTargets();
            var handles = bakeTargets.Select(x => new BakeTargetHandle<BakeTarget>(x)).Cast<IBakeTargetHandle>().ToList();
            handles.AddRange(bakingDescriptor.GetBakeTargetGroups().SelectMany(GetElementHandles));
            return handles;

            IEnumerable<IBakeTargetHandle> GetElementHandles(BakeTargetGroup g)
            {
                return g.GetAll().Select((_, i) => new BakeTargetGroupElementHandle<BakeTargetGroup>(g, i));
            }
        }

        static IBakingCommand[] CreateCommands(KanikamaBakeTargetDescriptor bakingDescriptor)
        {
            var commands = new List<IBakingCommand>();

            commands.AddRange(bakingDescriptor.GetBakeTargets().Select(x => new BakingCommand(new BakeTargetHandle<BakeTarget>(x))));
            commands.AddRange(bakingDescriptor.GetBakeTargetGroups().SelectMany(GetElementHandles).Select(h => new BakingCommand(h)));
            return commands.ToArray();

            IEnumerable<IBakeTargetHandle> GetElementHandles(BakeTargetGroup g)
            {
                return g.GetAll().Select((_, i) => new BakeTargetGroupElementHandle<BakeTargetGroup>(g, i));
            }
        }
    }
}
