using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    [RequireComponent(typeof(Camera)), UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KanikamaMapProvider : UdonSharpBehaviour
    {
        [SerializeField] Texture[] compositeTextures;
        [SerializeField] Material[] compositeMaterials;
        [SerializeField] KanikamaColorCollector colorCollector;
        [Space]
        [SerializeField] Renderer[] receivers;

        Vector4[] colors; // linear
        MaterialPropertyBlock block;


        void Start()
        {
            block = new MaterialPropertyBlock();
            foreach (var renderer in receivers)
            {
                var index = renderer.lightmapIndex;
                if (index < 0) continue;
                renderer.GetPropertyBlock(block);
                block.SetTexture("knkm_Lightmap", compositeTextures[index]);
                renderer.SetPropertyBlock(block);
            }

            colors = colorCollector.GetColors();
        }

        // Note:
        // Colors are updated by KanikamaColorCollector on OnPreCull,
        // then provided on OnPreRender.
        void OnPreRender()
        {
            // update materials
            foreach (var mat in compositeMaterials)
            {
                // No sRGB-linear conversion
                mat.SetVectorArray("knkm_Colors", colors);
            }
        }
    }
}