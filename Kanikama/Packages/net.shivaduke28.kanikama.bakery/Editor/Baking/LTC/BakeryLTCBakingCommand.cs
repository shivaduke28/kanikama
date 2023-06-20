using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baking.Experimental.LTC.Impl;
using Kanikama.Baking.Impl.LTC;
using Kanikama.Editor.Baking;
using Kanikama.Utility;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Bakery.Editor.Baking.LTC
{
    public sealed class BakeryLTCBakingCommand : IBakingCommand
    {
        readonly SceneObjectId sceneObjectId;
        ObjectHandle<KanikamaLTCMonitor> handle;
        public string Name { get; }
        public string Id => sceneObjectId.ToString();
        public string IdShadow => Id + "_shadow";
        public string IdLTC => Id + "_ltc";

        public BakeryLTCBakingCommand(KanikamaLTCMonitor value)
        {
            var globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(value);
            sceneObjectId = new SceneObjectId(globalObjectId);
            Name = value.name;
        }

        async Task IBakingCommand.RunAsync(BakeryBakingPipeline.Context context, CancellationToken cancellationToken)
        {
            // NOTE: BakeTargets are supposed to use Bakery Light Mesh.
            // Set Bounce 1 when BakeTargets Renderers with emissive materials.;
            var bounce = context.Lightmapper.Bounce;
            var dirMode = context.Lightmapper.DirectionalMode;
            context.Lightmapper.Bounce = 0;
            context.Lightmapper.DirectionalMode = BakeryLightmapper.DirMode.None;

            Debug.LogFormat(KanikamaDebug.Format, $"baking LTC monitor w/ shadow... name: {Name}, id: {Id}.");
            context.SceneGIContext.ClearCastShadow();
            context.Lightmapper.SetOutputAssetDirPath(context.Setting.OutputAssetDirPath);
            handle.Value.TurnOn();
            await context.Lightmapper.BakeAsync(cancellationToken);
            var bakedShadows = GetBakedLightmaps(context);
            var copiedShadow = BakeryBakingPipeline.CopyLightmaps(bakedShadows, context.Setting.OutputAssetDirPath, IdShadow);
            context.Setting.AssetStorage.LightmapStorage.AddOrUpdate(IdShadow, copiedShadow, Name + "_shadow");
            foreach (var lm in copiedShadow)
            {
                Debug.LogFormat(KanikamaDebug.Format, $"- copied lightmap: {lm.Path}");
            }

            Debug.LogFormat(KanikamaDebug.Format, $"baking LTC monitor w/o shadow... name: {Name}, id: {Id}.");
            context.SceneGIContext.SetCastShadowOff();
            context.Lightmapper.SetOutputAssetDirPath(context.Setting.OutputAssetDirPath);
            handle.Value.TurnOn();
            await context.Lightmapper.BakeAsync(cancellationToken);
            handle.Value.TurnOff();
            var bakedNoShadows = GetBakedLightmaps(context);
            var copiedNoShadow = BakeryBakingPipeline.CopyLightmaps(bakedNoShadows, context.Setting.OutputAssetDirPath, IdLTC);
            context.Setting.AssetStorage.LightmapStorage.AddOrUpdate(IdLTC, copiedNoShadow, Name + "_ltc");
            foreach (var lm in copiedNoShadow)
            {
                Debug.LogFormat(KanikamaDebug.Format, $"- copied lightmap: {lm.Path}");
            }
            context.Lightmapper.Bounce = bounce;
            context.Lightmapper.DirectionalMode = dirMode;
        }

        List<Lightmap> GetBakedLightmaps(BakeryBakingPipeline.Context context)
            => KanikamaBakeryUtility.GetLightmaps(context.Setting.OutputAssetDirPath, context.SceneAssetData.Asset.name)
                .Where(l => l.Type == BakeryLightmap.Light).ToList();

        void IBakingCommand.Initialize(string sceneGuid)
        {
            if (GlobalObjectIdHelper.TryParse(sceneGuid, 2, sceneObjectId.TargetObjectId, sceneObjectId.TargetPrefabId, out var globalObjectId))
            {
                handle = new ObjectHandle<KanikamaLTCMonitor>(globalObjectId);
            }
            handle.Value.Initialize();
            handle.Value.TurnOff();
        }

        void IBakingCommand.Clear()
        {
        }
    }
}
