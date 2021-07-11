
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kanikama.Udon
{
    public class KanikamaRenderer : UdonSharpBehaviour
    {
        [SerializeField] private Texture[] kanikamaMaps;
        [SerializeField] private Renderer renderer;

        void Start()
        {
            var index = renderer.lightmapIndex;
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

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!renderer) renderer = GetComponent<Renderer>();
        }

        public void SetKanikamaMaps(Texture[] maps)
        {
            kanikamaMaps = maps;
        }

        // todo
        // EditorOnlyでkanikamaMapsの更新処理
#endif
    }
}