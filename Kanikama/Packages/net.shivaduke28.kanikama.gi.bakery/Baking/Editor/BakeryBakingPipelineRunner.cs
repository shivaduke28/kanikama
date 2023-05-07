﻿using System.Collections.Generic;
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
            var handles = CreateHandles(bakingDescriptor);
            var sceneAssetData = new SceneAssetData(bakingSetting.SceneAsset);
            var settingAsset = BakeryBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);

            var ctx = new BakeryBakingPipeline.Context(
                sceneAssetData,
                handles,
                new BakeryLightmapper(),
                settingAsset.Setting
            );

            await BakeryBakingPipeline.BakeAsync(ctx, cancellationToken);
        }

        public static async Task RunWithoutKanikamaAsync(IBakingDescriptor bakingDescriptor,
            SceneAssetData sceneAssetData,
            CancellationToken cancellationToken)
        {
            var handles = CreateHandles(bakingDescriptor);
            var settingAsset = BakeryBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var context = new BakeryBakingPipeline.Context(sceneAssetData, handles, new BakeryLightmapper(), settingAsset.Setting);

            await BakeryBakingPipeline.BakeWithoutKanikamaAsync(context, cancellationToken);
        }

        public static void CreateAssets(IBakingDescriptor bakingDescriptor, SceneAssetData sceneAssetData)
        {
            var handles = CreateHandles(bakingDescriptor);
            var settingAsset = BakeryBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var setting = settingAsset.Setting;
            BakeryBakingPipeline.CreateAssets(handles, setting);
        }

        static List<IBakeTargetHandle> CreateHandles(IBakingDescriptor bakingDescriptor)
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
    }
}
