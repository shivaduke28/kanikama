using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Impl;

namespace Kanikama.Editor
{
    public sealed class UnityBakingPipelineRunner
    {
        public static async Task BakeAsync(KanikamaManager bakingDescriptor, SceneAssetData sceneAssetData, CancellationToken cancellationToken)
        {
            var commands = CreateCommands(bakingDescriptor);
            var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var parameter = new UnityBakingPipeline.Parameter(sceneAssetData, settingAsset.Setting, commands);
            await UnityBakingPipeline.BakeAsync(parameter, cancellationToken);
        }

        public static async Task BakeStaticAsync(KanikamaManager bakingDescriptor,
            SceneAssetData sceneAssetData,
            CancellationToken cancellationToken)
        {
            var commands = CreateCommands(bakingDescriptor);
            var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var parameter = new UnityBakingPipeline.Parameter(sceneAssetData, settingAsset.Setting, commands);
            await UnityBakingPipeline.BakeStaticAsync(parameter, cancellationToken);
        }

        public static void CreateAssets(KanikamaManager bakingDescriptor, SceneAssetData sceneAssetData)
        {
            var handles = CreateHandles(bakingDescriptor);
            var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var setting = settingAsset.Setting;
            UnityBakingPipeline.CreateAssets(handles, setting);
        }

        static List<IBakeTargetHandle> CreateHandles(KanikamaManager bakingDescriptor)
        {
            var bakeTargets = bakingDescriptor.GetBakeTargets();
            var handles = bakeTargets.Select(x => new BakeTargetHandle<KanikamaLightSource>(x)).Cast<IBakeTargetHandle>().ToList();
            handles.AddRange(bakingDescriptor.GetBakeTargetGroups().SelectMany(GetElementHandles));
            return handles;

            IEnumerable<IBakeTargetHandle> GetElementHandles(KanikamaLightSourceGroup g)
            {
                return g.GetAll().Select((_, i) => new BakeTargetGroupElementHandle<KanikamaLightSourceGroup>(g, i));
            }
        }

        static IUnityBakingCommand[] CreateCommands(KanikamaManager bakingDescriptor)
        {
            var commands = new List<IUnityBakingCommand>();

            commands.AddRange(bakingDescriptor.GetBakeTargets().Select(x => new UnityBakingCommand(new BakeTargetHandle<KanikamaLightSource>(x))));
            commands.AddRange(bakingDescriptor.GetBakeTargetGroups().SelectMany(GetElementHandles).Select(h => new UnityBakingCommand(h)));
            return commands.ToArray();

            IEnumerable<IBakeTargetHandle> GetElementHandles(KanikamaLightSourceGroup g)
            {
                return g.GetAll().Select((_, i) => new BakeTargetGroupElementHandle<KanikamaLightSourceGroup>(g, i));
            }
        }
    }
}
