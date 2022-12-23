
using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    [RequireComponent(typeof(Camera)), UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KanikamaMapArrayProvider : UdonSharpBehaviour
    {
        [SerializeField] Texture[] lightmapArrays;
        [SerializeField] Texture[] directionalLightmapArrays;
        [SerializeField] int sliceCount;
        [SerializeField] KanikamaColorCollector colorCollector;
        [Space]
        [SerializeField] Renderer[] receivers;

        Vector4[] colors; // linear
        MaterialPropertyBlock block;

        void Start()
        {
            block = new MaterialPropertyBlock();
            var directionalMapCount = directionalLightmapArrays == null ? -1 : directionalLightmapArrays.Length - 1;
            foreach (var renderer in receivers)
            {
                var index = renderer.lightmapIndex;
                if (index < 0) continue;
                renderer.GetPropertyBlock(block);
                block.SetTexture("knkm_LightmapArray", lightmapArrays[index]);
                if (index <= directionalMapCount)
                {
                    block.SetTexture("knkm_LightmapIndArray", directionalLightmapArrays[index]);
                }
                block.SetInt("knkm_Count", sliceCount);
                renderer.SetPropertyBlock(block);
            }

            colors = colorCollector.GetColors();
        }

        // Note:
        // Colors are updated by KanikamaColorCollector on OnPreCull,
        // then provided on OnPreRender.
        void OnPreRender()
        {
            // update renderers
            foreach (var renderer in receivers)
            {
                renderer.GetPropertyBlock(block);
                block.SetVectorArray("knkm_Colors", colors);
                renderer.SetPropertyBlock(block);
            }
        }
    }
}
