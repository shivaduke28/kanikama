using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kanikama.GI.Udon
{
    [RequireComponent(typeof(Camera)), UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [AddComponentMenu("Kanikama/Udon.KanikamaMapArrayProvider")]
    public class KanikamaMapArrayProvider : UdonSharpBehaviour
    {
        [SerializeField] Texture[] lightmapArrays;
        [SerializeField] Texture[] directionalLightmapArrays;
        [SerializeField] int sliceCount;
        [SerializeField] KanikamaColorCollector colorCollector;
        [Space] [SerializeField] Renderer[] receivers;

        Vector4[] colors; // linear
        MaterialPropertyBlock block;

        int lightmapArrayId;
        int lightmapIndArrayId;
        int countId;
        int colorsId;

        void Start()
        {
            lightmapArrayId = VRCShader.PropertyToID("_Udon_LightmapArray");
            lightmapIndArrayId = VRCShader.PropertyToID("_Udon_LightmapIndArray");
            countId = VRCShader.PropertyToID("_Udon_LightmapCount");
            colorsId = VRCShader.PropertyToID("_Udon_LightmapColors");

            block = new MaterialPropertyBlock();
            var directionalMapCount = directionalLightmapArrays == null ? -1 : directionalLightmapArrays.Length - 1;
            foreach (var renderer in receivers)
            {
                var index = renderer.lightmapIndex;
                if (index < 0) continue;
                renderer.GetPropertyBlock(block);
                block.SetTexture(lightmapArrayId, lightmapArrays[index]);
                if (index <= directionalMapCount)
                {
                    block.SetTexture(lightmapIndArrayId, directionalLightmapArrays[index]);
                }
                block.SetInt(countId, sliceCount);
                renderer.SetPropertyBlock(block);
            }

            colors = colorCollector.GetColors();
        }

        // Note:
        // Colors are updated by KanikamaColorCollector on OnPreCull,
        // then provided on OnPreRender.
        void OnPreRender()
        {
            VRCShader.SetGlobalVectorArray(colorsId, colors);
        }
    }
}
