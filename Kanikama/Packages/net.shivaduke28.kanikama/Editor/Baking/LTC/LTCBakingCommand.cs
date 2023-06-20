﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Baking.Impl.LTC;
using Kanikama.Utility;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.Baking.LTC
{
    public sealed class UnityLTCBakingCommand : IUnityBakingCommand
    {
        readonly SceneObjectId sceneObjectId;
        readonly string name;
        ObjectHandle<KanikamaLTCMonitor> handle;
        public string Name => name;
        public string Id => sceneObjectId.ToString();
        public string IdShadow => Id + "_shadow";
        public string IdLTC => Id + "_ltc";

        public UnityLTCBakingCommand(KanikamaLTCMonitor value)
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
            // TODO: use gi context
            // handle.Value.SetCastShadow(true);
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
            // TODO: use gi context
            // handle.Value.SetCastShadow(true);
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
                handle = new ObjectHandle<KanikamaLTCMonitor>(globalObjectId);
            }
            handle.Value.Initialize();
            handle.Value.TurnOff();
        }
    }
}
