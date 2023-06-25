using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Baking.Impl.LTC;
using Kanikama.Utility;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanikama.Editor.Baking.LTC
{
    public sealed class UnityLTCBakingCommand : IUnityBakingCommand
    {
        readonly SceneObjectId sceneObjectId;
        readonly string name;
        ObjectHandle<LTCMonitor> handle;
        public string Name => name;
        public string Id => sceneObjectId.ToString();
        public string IdShadow => Id + "_shadow";
        public string IdLTC => Id + "_ltc";

        public UnityLTCBakingCommand(LTCMonitor value)
        {
            var globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(value);
            sceneObjectId = new SceneObjectId(globalObjectId);
            name = value.name;
        }

        async Task IUnityBakingCommand.RunAsync(UnityBakingPipeline.Context context, CancellationToken cancellationToken)
        {
            var bounce = context.Lightmapper.Bounce;
            context.Lightmapper.Bounce = 0;
            Debug.LogFormat(KanikamaDebug.Format, $"baking LTC monitor w/ shadow... name: {name}, id: {Id}.");
            handle.Value.TurnOn();
            context.SceneGIContext.ClearCastShadow();

            //  NOTE: This is a workaround for that the Light status is not reflected in Unity's lightmapper.
            Selection.activeObject = handle.Value.Target;
            context.Lightmapper.ClearCache();
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
            handle.Value.TurnOff();
            var bakedNoShadows = UnityLightmapUtility.GetLightmaps(context.SceneAssetData)
                .Where(l => l.Type == UnityLightmap.Light)
                .ToList();
            var copiedNoShadow = UnityBakingPipeline.CopyBakedLightingAssetCollection(bakedNoShadows, context.Setting.OutputAssetDirPath, IdLTC);
            context.Setting.AssetStorage.LightmapStorage.AddOrUpdate(IdLTC, copiedNoShadow, name + "_ltc");
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
            handle.Value.Initialize();
        }

        public void TurnOff()
        {
            handle.Value.TurnOff();
        }
    }
}
