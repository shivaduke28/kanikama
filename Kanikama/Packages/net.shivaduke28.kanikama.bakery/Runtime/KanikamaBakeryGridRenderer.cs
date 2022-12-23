#if BAKERY_INCLUDED
using UnityEngine;

namespace Kanikama.Bakery
{
    [RequireComponent(typeof(Renderer), typeof(BakeryLightMesh))]
    public class KanikamaBakeryGridRenderer : KanikamaGridRenderer
    {
        [SerializeField] Renderer renderer;
        [SerializeField] BakeryLightMesh bakeryLightMesh;
        void OnValidate()
        {
            if (renderer == null)
            {
                renderer = GetComponent<Renderer>();
            }
            if (bakeryLightMesh == null)
            {
                bakeryLightMesh = GetComponent<BakeryLightMesh>();
            }
        }
        public override bool Contains(object obj)
        {
            return (obj is Renderer r && r == renderer) ||
             (obj is BakeryLightMesh m && m == bakeryLightMesh);
        }

        public override Renderer GetSource()
        {
            return renderer;
        }

        public override void OnBake()
        {
            renderer.enabled = true;
            bakeryLightMesh.enabled = true;
        }

        public override void OnBakeSceneStart()
        {
        }

        public override void Rollback()
        {
            renderer.enabled = false;
            bakeryLightMesh.enabled = true;
        }

        public override void TurnOff()
        {
            renderer.enabled = false;
            bakeryLightMesh.enabled = false;
        }
    }
}
#endif