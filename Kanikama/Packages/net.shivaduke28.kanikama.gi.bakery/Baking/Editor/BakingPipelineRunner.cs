using System.Linq;
using Kanikama.Core.Editor;
using Kanikama.GI.Baking;
using Kanikama.GI.Editor;
using UnityEditor;

namespace Kanikama.GI.Bakery.Editor
{
    public static class BakingPipelineRunner
    {
        [MenuItem("Kanikama/Bake with Bakery")]
        public static void Run()
        {
            if (!KanikamaSceneUtility.TryGetActiveSceneAsset(out var sceneAssetData)) return;
            var bakingDescriptor = KanikamaSceneUtility.FindObjectOfType<IBakingDescriptor>();
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
            var asset = BakeryBakingSettingAsset.Find(sceneAssetData.Asset);

            var ctx = new BakeryBakingPipeline.Context(
                sceneAssetData,
                handles,
                new BakeryLightmapper(),
                asset
            );

            var _ = BakeryBakingPipeline.BakeAsync(ctx, default);
        }
    }
}
