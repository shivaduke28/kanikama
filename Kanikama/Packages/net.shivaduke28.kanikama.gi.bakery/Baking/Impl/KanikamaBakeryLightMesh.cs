using Kanikama.GI.Baking;
using UnityEngine;

namespace Kanikama.GI.Bakery.Baking.Impl
{
    [RequireComponent(typeof(Renderer), typeof(BakeryLightMesh))]
    [AddComponentMenu("Kanikama/Baking.KanikamaBakeryLightMesh")]
    public sealed class KanikamaBakeryLightMesh : BakeTarget
    {
        [SerializeField] new Renderer renderer;
        [SerializeField] BakeryLightMesh bakeryLightMesh;
        [SerializeField] string tag;
        [SerializeField, HideInInspector] Color color;
        [SerializeField, HideInInspector] float intensity;

        void OnValidate()
        {
            if (renderer == null) renderer = GetComponent<Renderer>();
            if (bakeryLightMesh == null) bakeryLightMesh = GetComponent<BakeryLightMesh>();
        }

        public override void Initialize()
        {
            color = bakeryLightMesh.color;
            intensity = bakeryLightMesh.intensity;
            tag = gameObject.tag;
            gameObject.tag = "Untagged";
        }

        public override void TurnOff()
        {
            renderer.enabled = false;
            bakeryLightMesh.enabled = false;
        }

        public override void TurnOn()
        {
            renderer.enabled = true;
            bakeryLightMesh.enabled = true;
            bakeryLightMesh.color = Color.white;
            bakeryLightMesh.intensity = 1f;
        }

        public override bool Includes(Object obj)
        {
            return obj is Renderer r && r == renderer || obj is BakeryLightMesh m && m == bakeryLightMesh;
        }

        public override void Clear()
        {
            renderer.enabled = true;
            bakeryLightMesh.enabled = true;
            bakeryLightMesh.color = color;
            bakeryLightMesh.intensity = intensity;
            gameObject.tag = tag;
        }
    }
}
