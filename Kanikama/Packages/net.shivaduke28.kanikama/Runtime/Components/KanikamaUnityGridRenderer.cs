using UnityEngine;

namespace Kanikama
{
    [RequireComponent(typeof(Renderer))]
    public class KanikamaUnityGridRenderer : KanikamaGridRenderer
    {
        [SerializeField] Renderer renderer;
        void OnValidate()
        {
            if (renderer == null)
            {
                renderer = GetComponent<Renderer>();
            }
        }
        public override bool Contains(object obj)
        {
            return obj is Renderer r && r == renderer;
        }

        public override Renderer GetSource()
        {
            return renderer;
        }

        public override void OnBake()
        {
            renderer.enabled = true;
        }

        public override void OnBakeSceneStart()
        {
        }

        public override void Rollback()
        {
            renderer.enabled = false;
        }

        public override void TurnOff()
        {
            renderer.enabled = false;
        }
    }
}
