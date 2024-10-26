using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Kanikama.Bakery
{
    [RequireComponent(typeof(BakeryLightMesh))]
    public sealed class KanikamaBakeryKanikamaLtcMonitor : KanikamaLtcMonitor
    {
        [SerializeField] new Renderer renderer;
        [SerializeField] BakeryLightMesh bakeryLightMesh;

        void OnValidate()
        {
            if (renderer == null) renderer = GetComponent<Renderer>();
            if (bakeryLightMesh == null) bakeryLightMesh = GetComponent<BakeryLightMesh>();
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public override void Initialize()
        {
        }

        public override void TurnOff()
        {
            bakeryLightMesh.intensity = 0;
        }

        public override void TurnOn()
        {
            renderer.shadowCastingMode = ShadowCastingMode.On;
            bakeryLightMesh.selfShadow = true;
            bakeryLightMesh.enabled = true;
            bakeryLightMesh.color = Color.white;
            bakeryLightMesh.intensity = 1f;
        }

        public bool Includes(Object obj)
        {
            return obj == renderer || obj == bakeryLightMesh;
        }

        public override void Clear()
        {
        }
#endif
    }
}
