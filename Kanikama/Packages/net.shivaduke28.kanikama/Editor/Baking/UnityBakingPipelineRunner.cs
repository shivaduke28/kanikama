using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Baking;

namespace Kanikama.Editor.Baking
{
    public sealed class UnityBakingPipelineRunner
    {
        public static async Task BakeAsync(IBakingDescriptor bakingDescriptor, SceneAssetData sceneAssetData, CancellationToken cancellationToken)
        {
            var handles = CreateHandles(bakingDescriptor);
            var commands = handles.Select(h => new UnityBakingCommand(h)).Cast<IUnityBakingCommand>().ToList();
            var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var context = new UnityBakingPipeline.Parameter(sceneAssetData, settingAsset.Setting, commands);
            await UnityBakingPipeline.BakeAsync(context, cancellationToken);
        }

        public static async Task BakeStaticAsync(IBakingDescriptor bakingDescriptor,
            SceneAssetData sceneAssetData,
            CancellationToken cancellationToken)
        {
            var handles = CreateHandles(bakingDescriptor);
            var commands = handles.Select(h => new UnityBakingCommand(h)).Cast<IUnityBakingCommand>().ToList();
            var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);
            var context = new UnityBakingPipeline.Parameter(sceneAssetData, settingAsset.Setting, commands);

            await UnityBakingPipeline.BakeStaticAsync(context, cancellationToken);
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
