using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Core;
using Kanikama.Core.Editor;
using Kanikama.GI.Baking;
using UnityEngine;

namespace Kanikama.GI.Editor
{
    public sealed class BakingPipelineRunner
    {
        public static async Task RunAsync(IBakingDescriptor bakingDescriptor, SceneAssetData sceneAssetData, CancellationToken cancellationToken)
        {
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

            var settingAsset = UnityBakingSettingAsset.FindOrCreate(sceneAssetData.Asset);

            var context = new UnityBakingPipeline.BakingContext(sceneAssetData, handles, new UnityLightmapper(), settingAsset.Setting);

            await UnityBakingPipeline.BakeAsync(context, cancellationToken);
        }

        public static async Task RunWithoutKanikamaAsync(IBakingDescriptor bakingDescriptor,
            SceneAssetData sceneAssetData,
            CancellationToken cancellationToken)
        {
            if (!UnityBakingSettingAsset.TryFind(sceneAssetData.Asset, out var settingAsset))
            {
                Debug.LogErrorFormat(KanikamaDebug.Format, $"{nameof(UnityBakingSettingAsset)} is not found.");
                return;
            }

            var bakeTargets = bakingDescriptor.GetBakeTargets();
            var handles = bakeTargets.Select(x => new BakeTargetHandle<BakeTarget>(x)).Cast<IBakeTargetHandle>().ToList();
            var context = new UnityBakingPipeline.BakingContext(sceneAssetData, handles, new UnityLightmapper(), settingAsset.Setting);

            await UnityBakingPipeline.BakeWithoutKanikamaAsync(context, cancellationToken);
        }
    }
}
