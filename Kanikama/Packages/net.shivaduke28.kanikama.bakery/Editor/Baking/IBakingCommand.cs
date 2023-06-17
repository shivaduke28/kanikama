using System.Threading;
using System.Threading.Tasks;
using Kanikama.Editor.Baking;
using Kanikama.Utility;
using UnityEngine;

namespace Kanikama.Bakery.Editor.Baking
{
    public interface IBakingCommand
    {
        Task RunAsync(BakeryBakingPipeline.Context context, CancellationToken cancellationToken);
        void Initialize(string sceneGuid);
        void Clear();
    }

    public sealed class BakingCommand : IBakingCommand
    {
        readonly IBakeTargetHandle handle;

        public BakingCommand(IBakeTargetHandle handle)
        {
            this.handle = handle;
        }

        async Task IBakingCommand.RunAsync(BakeryBakingPipeline.Context context, CancellationToken cancellationToken)
        {
            Debug.LogFormat(KanikamaDebug.Format, $"baking... name: {handle.Name}, id: {handle.Id}.");
            handle.TurnOn();
            // NOTE: need to set output path explicitly to Bakery.
            context.Lightmapper.SetOutputAssetDirPath(context.Setting.OutputAssetDirPath);
            await context.Lightmapper.BakeAsync(cancellationToken);
            handle.TurnOff();

            var baked = KanikamaBakeryUtility.GetLightmaps(context.Setting.OutputAssetDirPath, context.SceneAssetData.Asset.name);
            var copied = BakeryBakingPipeline.CopyLightmaps(baked, context.Setting.OutputAssetDirPath, handle.Id);
            context.Setting.AssetStorage.LightmapStorage.AddOrUpdate(handle.Id, copied, handle.Name);
            foreach (var lm in copied)
            {
                Debug.LogFormat(KanikamaDebug.Format, $"- copied lightmap: {lm.Path}");
            }
        }

        void IBakingCommand.Initialize(string sceneGuid)
        {
            handle.Initialize(sceneGuid);
            handle.TurnOff();
        }

        void IBakingCommand.Clear()
        {
            handle.Clear();
        }
    }
}
