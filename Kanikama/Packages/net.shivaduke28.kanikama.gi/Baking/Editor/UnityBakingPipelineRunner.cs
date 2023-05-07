﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core.Editor;
using Kanikama.GI.Baking;

namespace Kanikama.GI.Editor
{
    public sealed class UnityBakingPipelineRunner
    {
        public static async Task RunAsync(IBakingDescriptor bakingDescriptor, SceneAssetData sceneAssetData, CancellationToken cancellationToken)
        {
            var handles = CreateHandles(bakingDescriptor);
            var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var context = new UnityBakingPipeline.BakingContext(sceneAssetData, handles, new UnityLightmapper(), settingAsset.Setting);
            await UnityBakingPipeline.BakeAsync(context, cancellationToken);
        }

        public static async Task RunWithoutKanikamaAsync(IBakingDescriptor bakingDescriptor,
            SceneAssetData sceneAssetData,
            CancellationToken cancellationToken)
        {
            var handles = CreateHandles(bakingDescriptor);
            var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var context = new UnityBakingPipeline.BakingContext(sceneAssetData, handles, new UnityLightmapper(), settingAsset.Setting);

            await UnityBakingPipeline.BakeWithoutKanikamaAsync(context, cancellationToken);
        }

        public static void CreateAssets(IBakingDescriptor bakingDescriptor, SceneAssetData sceneAssetData)
        {
            var handles = CreateHandles(bakingDescriptor);
            var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var setting = settingAsset.Setting;
            UnityBakingPipeline.CreateAssets(handles, setting);
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
