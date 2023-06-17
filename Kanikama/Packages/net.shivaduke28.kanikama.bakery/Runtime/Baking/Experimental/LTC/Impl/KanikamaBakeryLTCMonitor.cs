using UnityEngine;
using UnityEngine.Rendering;

namespace Baking.Experimental.LTC.Impl
{
    [RequireComponent(typeof(BakeryLightMesh))]
    [RequireComponent(typeof(Renderer))]
    public sealed class KanikamaBakeryLTCMonitor : MonoBehaviour
    {
        [SerializeField] BakeryLightMesh bakeryLightMesh;
        [SerializeField] new Renderer renderer;

        void OnValidate() => Initialize();

        public void Initialize()
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

        public void TurnOn()
        {
            renderer.shadowCastingMode = ShadowCastingMode.On;
            bakeryLightMesh.selfShadow = true;
            bakeryLightMesh.enabled = true;
        }

        public void TurnOff()
        {
            bakeryLightMesh.enabled = false;
        }
    }
}
