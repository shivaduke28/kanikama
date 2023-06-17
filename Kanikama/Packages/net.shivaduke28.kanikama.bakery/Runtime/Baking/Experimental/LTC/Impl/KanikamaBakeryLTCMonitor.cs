using Kanikama.Baking.Experimental.LTC;
using UnityEngine;
using UnityEngine.Rendering;

namespace Baking.Experimental.LTC.Impl
{
    [RequireComponent(typeof(BakeryLightMesh))]
    [RequireComponent(typeof(Renderer))]
    public sealed class KanikamaBakeryLTCMonitor : LTCMonitor
    {
        [SerializeField] BakeryLightMesh bakeryLightMesh;
        [SerializeField] new Renderer renderer;

        void OnValidate()
        {
            if (bakeryLightMesh == null)
            {
                bakeryLightMesh = GetComponent<BakeryLightMesh>();
            }
            if (renderer == null)
            {
                renderer = GetComponent<Renderer>();
            }
        }

        public override void Initialize()
        {
        }

        public override void TurnOn()
        {
            renderer.shadowCastingMode = ShadowCastingMode.On;
            bakeryLightMesh.enabled = true;
        }

        public override void TurnOff()
        {
            bakeryLightMesh.enabled = false;
        }

        public override void SetCastShadow(bool enable)
        {
        }
    }
}
