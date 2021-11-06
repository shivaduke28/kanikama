
using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    [RequireComponent(typeof(Camera)), UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KanikamaMapArrayProvider : UdonSharpBehaviour
    {
        [SerializeField] Texture[] lightmapArrays;
        [SerializeField] Texture[] directionalLightmapArrays;
        [SerializeField] int lightmapCount;
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
                block.SetTexture("_LightmapArray", lightmapArrays[index]);
                if (index <= directionalMapCount)
                {
                    block.SetTexture("_DirectionalLightmapArray", directionalLightmapArrays[index]);
                }
                block.SetInt("_LightmapCount", lightmapCount);
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
                block.SetVectorArray("_LightmapColors", colors);
                renderer.SetPropertyBlock(block);
            }
        }
    }
}
