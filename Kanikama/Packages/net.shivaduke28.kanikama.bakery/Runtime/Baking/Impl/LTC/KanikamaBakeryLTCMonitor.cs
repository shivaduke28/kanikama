using Kanikama.Baking.Impl.LTC;
using Kanikama.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Baking.Impl.LTC
{
    public sealed class KanikamaBakeryLTCMonitor : KanikamaLTCMonitor
    {
        [SerializeField] BakeryLightMesh bakeryLightMesh;
        [SerializeField] new Renderer renderer;

        void OnValidate()
        {
            Initialize();
        }

        public override void Initialize()
        {
            if (bakeryLightMesh == null)
            {
                var child = transform.Find("BakeryLTCLight");
                if (child == null)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    if (go.TryGetComponent<Collider>(out var c))
                    {
                        c.DestroySafely();
                    }
                    go.name = "BakeryLTCLight";
                    go.transform.SetParent(transform, false);
                    go.transform.localPosition = new Vector3(0, 0, -0.001f);
                    go.transform.localRotation = Quaternion.identity;
                    child = go.transform;
                }
                renderer = child.GetComponent<Renderer>();
                bakeryLightMesh = child.gameObject.GetOrAddComponent<BakeryLightMesh>();
                TurnOff();
            }
        }

        public override void TurnOn()
        {
            renderer.shadowCastingMode = ShadowCastingMode.On;
            bakeryLightMesh.selfShadow = true;
            bakeryLightMesh.enabled = true;
            bakeryLightMesh.color = Color.white;
            bakeryLightMesh.intensity = 1;
        }

        public override void TurnOff()
        {
            bakeryLightMesh.enabled = false;
            renderer.enabled = false;
        }
    }
}
