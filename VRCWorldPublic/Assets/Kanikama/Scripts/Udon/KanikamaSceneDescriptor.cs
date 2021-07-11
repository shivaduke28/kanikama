
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kanikama.Udon
{
    public class KanikamaSceneDescriptor : UdonSharpBehaviour
    {
        [SerializeField] private Light[] lights;
        [SerializeField] private Renderer[] emissiveRenderers;
        [Space]
        [SerializeField] private Texture[] lightMaps;
        [Space]
        [SerializeField] private Renderer[] receivers;

        private void Start()
        {
            foreach (var renderer in receivers)
            {
                var index = renderer.lightmapIndex;
                var sharedMats = renderer.sharedMaterials;
                for (var i = 0; i < sharedMats.Length; i++)
                {
                    var mat = sharedMats[i];
                    if (mat.HasProperty("_KanikamaMap"))
                    {
                        var p = new MaterialPropertyBlock();
                        p.SetTexture("_KanikamaMap", lightMaps[index]);
                        renderer.SetPropertyBlock(p, i);
                    }
                }
            }
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public Light[] Lights => lights;
        public Renderer[] EmissiveReceivers => emissiveRenderers;
        public Texture[] LightMaps => lightMaps;
        public Renderer[] Receivers => receivers;


#endif
    }
}
