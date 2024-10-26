using UnityEngine;
using UnityEngine.Rendering;

namespace Kanikama.Bakery
{
    [RequireComponent(typeof(BakeryLightMesh))]
    public sealed class KanikamaBakeryLtcMonitor : KanikamaLtcMonitor
    {
        [SerializeField] new Renderer renderer;

        void OnValidate()
        {
            if (renderer == null) renderer = GetComponent<Renderer>();
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        BakeryLightMesh BakeryLightMesh => GetComponent<BakeryLightMesh>();

        public override void Initialize()
        {
        }

        public override void TurnOff()
        {
            BakeryLightMesh.intensity = 0;
        }

        public override void TurnOn()
        {
            renderer.shadowCastingMode = ShadowCastingMode.On;
            BakeryLightMesh.selfShadow = true;
            BakeryLightMesh.enabled = true;
            BakeryLightMesh.color = Color.white;
            BakeryLightMesh.intensity = 1f;
        }

        public override void Clear()
        {
        }
#endif
    }
}
