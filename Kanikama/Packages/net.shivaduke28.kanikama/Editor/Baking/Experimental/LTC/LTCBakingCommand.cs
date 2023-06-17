using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Baking.Experimental.LTC;
using Kanikama.Utility;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.Baking.Experimental.LTC
{
    public sealed class LTCBakingCommand : IUnityBakingCommand
    {
        readonly SceneObjectId sceneObjectId;
        readonly string name;
        ObjectHandle<LTCMonitor> handle;
        string Id => sceneObjectId.ToString();
        string IdShadow => Id + "_shadow";
        string IdLTC => Id + "_ltc";

        public LTCBakingCommand(LTCMonitor value)
        {
            var globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(value);
            sceneObjectId = new SceneObjectId(globalObjectId);
            name = value.name;
        }

        async Task IUnityBakingCommand.RunAsync(UnityBakingPipeline.Context context, CancellationToken cancellationToken)
        {
            var bounce = context.Lightmapper.Bounce;
            // NOTE: BakeTargets are supposed to use Unity Area Light.
            // Set Bounce 1 when BakeTargets Renderers with emissive materials.;
            context.Lightmapper.Bounce = 0;
            Debug.LogFormat(KanikamaDebug.Format, $"baking LTC monitor w/ shadow... name: {name}, id: {Id}.");
            handle.Value.TurnOn();
            handle.Value.SetCastShadow(true);
            context.Lightmapper.ClearCache();
            await context.Lightmapper.BakeAsync(cancellationToken);
            var bakedShadows = UnityLightmapUtility.GetLightmaps(context.SceneAssetData)
                .Where(l => l.Type == UnityLightmapType.Light)
                .ToList();
            var copiedShadow = UnityBakingPipeline.CopyBakedLightingAssetCollection(bakedShadows, context.Setting.OutputAssetDirPath,
                IdShadow);
            context.Setting.LightmapStorage.AddOrUpdate(IdShadow, copiedShadow, name + "_shadow");
            foreach (var lm in copiedShadow)
            {
                Debug.LogFormat(KanikamaDebug.Format, $"- copied lightmap: {lm.Path}");
            }

            Debug.LogFormat(KanikamaDebug.Format, $"baking LTC monitor w/o shadow... name: {name}, id: {Id}.");
            handle.Value.SetCastShadow(false);
            context.Lightmapper.ClearCache();
            await context.Lightmapper.BakeAsync(cancellationToken);
            handle.Value.TurnOff();
            var bakedNoShadows = UnityLightmapUtility.GetLightmaps(context.SceneAssetData)
                .Where(l => l.Type == UnityLightmapType.Light)
                .ToList();
            var copiedNoShadow = UnityBakingPipeline.CopyBakedLightingAssetCollection(bakedNoShadows, context.Setting.OutputAssetDirPath, IdLTC);
            context.Setting.LightmapStorage.AddOrUpdate(IdLTC, copiedNoShadow, name + "_ltc");
            foreach (var lm in copiedNoShadow)
            {
                Debug.LogFormat(KanikamaDebug.Format, $"- copied lightmap: {lm.Path}");
            }
            context.Lightmapper.Bounce = bounce;
        }

        void IUnityBakingCommand.Initialize(string sceneGuid)
        {
            if (GlobalObjectIdHelper.TryParse(sceneGuid, 2, sceneObjectId.TargetObjectId, sceneObjectId.TargetPrefabId, out var globalObjectId))
            {
                handle = new ObjectHandle<LTCMonitor>(globalObjectId);
            }
        }
    }
}
