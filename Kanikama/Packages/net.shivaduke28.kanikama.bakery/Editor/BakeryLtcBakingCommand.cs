using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Editor;
using Kanikama.Utility;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Bakery.Editor.LTC
{
    public sealed class BakeryLtcBakingCommand : IBakingCommand
    {
        readonly SceneObjectId sceneObjectId;
        ObjectHandle<KanikamaLtcMonitor> handle;
        public string Name { get; }
        public string Id => sceneObjectId.ToString();
        public string IdShadow => Id + "_shadow";
        public string IdLtc => Id + "_ltc";

        public BakeryLtcBakingCommand(KanikamaLtcMonitor value)
        {
            var globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(value);
            sceneObjectId = new SceneObjectId(globalObjectId);
            Name = value.name;
        }

        async Task IBakingCommand.RunAsync(BakeryBakingPipeline.Context context, CancellationToken cancellationToken)
        {
            // NOTE: BakeTargets are supposed to use Bakery Light Mesh.
            // Set Bounce 1 when BakeTargets Renderers with emissive materials.;
            var renderMode = context.Lightmapper.LightmapRenderMode;
            var bounce = context.Lightmapper.Bounce;
            var dirMode = context.Lightmapper.DirectionalMode;

            // RenderMode must be FullLighting when Bounce < 1
            context.Lightmapper.LightmapRenderMode = BakeryLightmapper.RenderMode.FullLighting;
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
            var copiedNoShadow = BakeryBakingPipeline.CopyLightmaps(bakedNoShadows, context.Setting.OutputAssetDirPath, IdLtc);
            context.Setting.AssetStorage.LightmapStorage.AddOrUpdate(IdLtc, copiedNoShadow, Name + "_ltc");
            foreach (var lm in copiedNoShadow)
            {
                Debug.LogFormat(KanikamaDebug.Format, $"- copied lightmap: {lm.Path}");
            }
            context.Lightmapper.Bounce = bounce;
            context.Lightmapper.DirectionalMode = dirMode;
            context.Lightmapper.LightmapRenderMode = renderMode;
        }

        List<Lightmap> GetBakedLightmaps(BakeryBakingPipeline.Context context)
            => KanikamaBakeryUtility.GetLightmaps()
                .Where(l => l.Type == BakeryLightmap.Light).ToList();

        void IBakingCommand.Initialize(string sceneGuid)
        {
            if (GlobalObjectIdUtility.TryParse(sceneGuid, 2, sceneObjectId.TargetObjectId, sceneObjectId.TargetPrefabId, out var globalObjectId))
            {
                handle = new ObjectHandle<KanikamaLtcMonitor>(globalObjectId);
            }
            handle.Value.Initialize();
        }

        public void TurnOff() => handle.Value.TurnOff();

        void IBakingCommand.Clear()
        {
            handle.Value.Clear();
        }
    }
}
