using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Kanikama.Editor
{
    public sealed class UnityLtcBakingCommand : IUnityBakingCommand
    {
        readonly SceneObjectId sceneObjectId;
        readonly string name;
        readonly IBakeTargetHandle handle;
        public string Name => name;
        public string Id => sceneObjectId.ToString();
        public string IdShadow => Id + "_shadow";
        public string IdLtc => Id + "_ltc";

        public UnityLtcBakingCommand(IBakeTargetHandle bakeTargetHandle)
        {
            handle = bakeTargetHandle;
        }

        async Task IUnityBakingCommand.RunAsync(UnityBakingPipeline.Context context, CancellationToken cancellationToken)
        {
            var bounce = context.Lightmapper.Bounce;
            context.Lightmapper.Bounce = 0;
            Debug.LogFormat(KanikamaDebug.Format, $"baking LTC monitor w/ shadow... name: {name}, id: {Id}.");
            context.SceneGIContext.ClearCastShadow();
            context.Lightmapper.ClearCache();
            handle.TurnOn();
            await context.Lightmapper.BakeAsync(cancellationToken);
            var bakedShadows = UnityLightmapUtility.GetLightmaps(context.SceneAssetData)
                .Where(l => l.Type == UnityLightmap.Light)
                .ToList();
            var copiedShadow = UnityBakingPipeline.CopyBakedLightingAssetCollection(bakedShadows, context.Setting.OutputAssetDirPath,
                IdShadow);
            context.Setting.AssetStorage.LightmapStorage.AddOrUpdate(IdShadow, copiedShadow, name + "_shadow");
            foreach (var lm in copiedShadow)
            {
                Debug.LogFormat(KanikamaDebug.Format, $"- copied lightmap: {lm.Path}");
            }

            Debug.LogFormat(KanikamaDebug.Format, $"baking LTC monitor w/o shadow... name: {name}, id: {Id}.");
            context.SceneGIContext.SetCastShadowOff();
            context.Lightmapper.ClearCache();
            await context.Lightmapper.BakeAsync(cancellationToken);
            handle.TurnOff();
            var bakedNoShadows = UnityLightmapUtility.GetLightmaps(context.SceneAssetData)
                .Where(l => l.Type == UnityLightmap.Light)
                .ToList();
            var copiedNoShadow = UnityBakingPipeline.CopyBakedLightingAssetCollection(bakedNoShadows, context.Setting.OutputAssetDirPath, IdLtc);
            context.Setting.AssetStorage.LightmapStorage.AddOrUpdate(IdLtc, copiedNoShadow, name + "_ltc");
            foreach (var lm in copiedNoShadow)
            {
                Debug.LogFormat(KanikamaDebug.Format, $"- copied lightmap: {lm.Path}");
            }
            context.Lightmapper.Bounce = bounce;
        }

        void IUnityBakingCommand.Initialize(string sceneGuid)
        {
            handle.Initialize(sceneGuid);
        }

        public void TurnOff()
        {
            handle.TurnOff();
        }
    }
}
