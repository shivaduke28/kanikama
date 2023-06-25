using System.Threading;
using System.Threading.Tasks;
using Kanikama.Utility;
using UnityEngine;

namespace Kanikama.Editor.Baking
{
    public sealed class UnityBakingCommand : IUnityBakingCommand
    {
        readonly IBakeTargetHandle handle;

        public UnityBakingCommand(IBakeTargetHandle handle)
        {
            this.handle = handle;
        }

        async Task IUnityBakingCommand.RunAsync(UnityBakingPipeline.Context context, CancellationToken cancellationToken)
        {
            Debug.LogFormat(KanikamaDebug.Format, $"baking... name: {handle.Name}, id: {handle.Id}.");
            handle.TurnOn();
            context.Lightmapper.ClearCache();
            await context.Lightmapper.BakeAsync(cancellationToken);
            handle.TurnOff();

            var baked = UnityLightmapUtility.GetLightmaps(context.SceneAssetData);
            var copied = UnityBakingPipeline.CopyBakedLightingAssetCollection(baked, context.Setting.OutputAssetDirPath, handle.Id);
            context.Setting.AssetStorage.LightmapStorage.AddOrUpdate(handle.Id, copied, handle.Name);
            foreach (var lm in copied)
            {
                Debug.LogFormat(KanikamaDebug.Format, $"- copied lightmap: {lm.Path}");
            }
        }

        void IUnityBakingCommand.Initialize(string sceneGuid)
        {
            handle.Initialize(sceneGuid);
        }

        void IUnityBakingCommand.TurnOff() => handle.TurnOff();
    }

    public interface IUnityBakingCommand
    {
        Task RunAsync(UnityBakingPipeline.Context context, CancellationToken cancellationToken);
        void Initialize(string sceneGuid);
        void TurnOff();
    }
}
