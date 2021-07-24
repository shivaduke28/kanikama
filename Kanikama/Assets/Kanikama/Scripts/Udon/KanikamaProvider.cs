
using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    public class KanikamaProvider : UdonSharpBehaviour
    {
        [SerializeField] private Texture[] kanikamaMaps;
        [SerializeField] private Renderer[] receivers;

        private void Start()
        {
            if (kanikamaMaps.Length == 0) return;
            foreach (var renderer in receivers)
            {
                var index = renderer.lightmapIndex;
                if (index < 0) continue;
                var sharedMats = renderer.sharedMaterials;
                for (var i = 0; i < sharedMats.Length; i++)
                {
                    var mat = sharedMats[i];
                    if (mat.HasProperty("_KanikamaMap"))
                    {
                        var p = new MaterialPropertyBlock();
                        p.SetTexture("_KanikamaMap", kanikamaMaps[index]);
                        renderer.SetPropertyBlock(p, i);
                    }
                }
            }
        }
    }
}
