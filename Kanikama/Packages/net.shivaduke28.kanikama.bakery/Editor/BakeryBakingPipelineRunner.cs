using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Components;
using Kanikama.Editor;

namespace Kanikama.Bakery.Editor
{
    public static class BakeryBakingPipelineRunner
    {
        public static async Task BakeAsync(KanikamaManager bakingDescriptor, BakeryBakingSetting bakingSetting,
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

        public static async Task BakeStaticAsync(KanikamaManager bakingDescriptor,
            SceneAssetData sceneAssetData,
            CancellationToken cancellationToken)
        {
            var commands = CreateCommands(bakingDescriptor);
            var settingAsset = BakeryBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var context = new BakeryBakingPipeline.Parameter(sceneAssetData, settingAsset.Setting, commands);

            await BakeryBakingPipeline.BakeStaticAsync(context, cancellationToken);
        }

        public static void CreateAssets(KanikamaManager bakingDescriptor, SceneAssetData sceneAssetData)
        {
            var handles = CreateHandles(bakingDescriptor);
            var settingAsset = BakeryBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var setting = settingAsset.Setting;
            BakeryBakingPipeline.CreateAssets(handles, setting);
        }

        static List<IBakeTargetHandle> CreateHandles(KanikamaManager bakingDescriptor)
        {
            throw new NotSupportedException();
            // var bakeTargets = bakingDescriptor.GetBakeTargets();
            // var handles = bakeTargets.Select(x => new BakeTargetHandle<BakeTarget>(x)).Cast<IBakeTargetHandle>().ToList();
            // handles.AddRange(bakingDescriptor.GetBakeTargetGroups().SelectMany(GetElementHandles));
            // return handles;
            //
            // IEnumerable<IBakeTargetHandle> GetElementHandles(BakeTargetGroup g)
            // {
            //     return g.GetAll().Select((_, i) => new BakeTargetGroupElementHandle<BakeTargetGroup>(g, i));
            // }
        }

        static IBakingCommand[] CreateCommands(KanikamaManager bakingDescriptor)
        {
            throw new NotSupportedException();
            // var commands = new List<IBakingCommand>();
            //
            // commands.AddRange(bakingDescriptor.GetBakeTargets().Select(x => new BakingCommand(new BakeTargetHandle<BakeTarget>(x))));
            // commands.AddRange(bakingDescriptor.GetBakeTargetGroups().SelectMany(GetElementHandles).Select(h => new BakingCommand(h)));
            // return commands.ToArray();
            //
            // IEnumerable<IBakeTargetHandle> GetElementHandles(BakeTargetGroup g)
            // {
            //     return g.GetAll().Select((_, i) => new BakeTargetGroupElementHandle<BakeTargetGroup>(g, i));
            // }
        }
    }
}
