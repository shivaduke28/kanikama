﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Baking;
using Kanikama.Baking.Impl;
using Kanikama.Baking.Impl.LTC;
using Kanikama.Editor.Baking.LTC;

namespace Kanikama.Editor.Baking
{
    public sealed class UnityBakingPipelineRunner
    {
        public static async Task BakeAsync(KanikamaBakeTargetDescriptor bakingDescriptor, SceneAssetData sceneAssetData, CancellationToken cancellationToken)
        {
            var commands = CreateCommands(bakingDescriptor);
            var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var parameter = new UnityBakingPipeline.Parameter(sceneAssetData, settingAsset.Setting, commands);
            await UnityBakingPipeline.BakeAsync(parameter, cancellationToken);
        }

        public static async Task BakeStaticAsync(KanikamaBakeTargetDescriptor bakingDescriptor,
            SceneAssetData sceneAssetData,
            CancellationToken cancellationToken)
        {
            var commands = CreateCommands(bakingDescriptor);
            var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var parameter = new UnityBakingPipeline.Parameter(sceneAssetData, settingAsset.Setting, commands);
            await UnityBakingPipeline.BakeStaticAsync(parameter, cancellationToken);
        }

        public static void CreateAssets(KanikamaBakeTargetDescriptor bakingDescriptor, SceneAssetData sceneAssetData)
        {
            var handles = CreateHandles(bakingDescriptor);
            var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var setting = settingAsset.Setting;
            UnityBakingPipeline.CreateAssets(handles, setting);
        }

        static List<IBakeTargetHandle> CreateHandles(KanikamaBakeTargetDescriptor bakingDescriptor)
        {
            var bakeTargets = bakingDescriptor.GetBakeTargets();
            var handles = bakeTargets.Select(x => new BakeTargetHandle<BakeTarget>(x)).Cast<IBakeTargetHandle>().ToList();
            handles.AddRange(bakingDescriptor.GetBakeTargetGroups().SelectMany(GetElementHandles));
            handles.AddRange(bakingDescriptor.GetLTCMonitors().Take(3).Select(x => new BakeTargetHandle<LTCMonitor>(x)));
            return handles;

            IEnumerable<IBakeTargetHandle> GetElementHandles(BakeTargetGroup g)
            {
                return g.GetAll().Select((_, i) => new BakeTargetGroupElementHandle<BakeTargetGroup>(g, i));
            }
        }

        static IUnityBakingCommand[] CreateCommands(KanikamaBakeTargetDescriptor bakingDescriptor)
        {
            var commands = new List<IUnityBakingCommand>();

            commands.AddRange(bakingDescriptor.GetBakeTargets().Select(x => new UnityBakingCommand(new BakeTargetHandle<BakeTarget>(x))));
            commands.AddRange(bakingDescriptor.GetBakeTargetGroups().SelectMany(GetElementHandles).Select(h => new UnityBakingCommand(h)));
            commands.AddRange(bakingDescriptor.GetLTCMonitors().Take(3).Select(x => new UnityLTCBakingCommand(x)));
            return commands.ToArray();

            IEnumerable<IBakeTargetHandle> GetElementHandles(BakeTargetGroup g)
            {
                return g.GetAll().Select((_, i) => new BakeTargetGroupElementHandle<BakeTargetGroup>(g, i));
            }
        }
    }
}
