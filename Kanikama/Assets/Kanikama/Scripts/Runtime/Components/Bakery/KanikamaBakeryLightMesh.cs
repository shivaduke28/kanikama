#if BAKERY_INCLUDED
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.Bakery
{
    [RequireComponent(typeof(Renderer), typeof(BakeryLightMesh))]
    public class KanikamaBakeryLightMesh : KanikamaRenderer, ILightSource
    {
        [SerializeField] Renderer renderer;
        [SerializeField] BakeryLightMesh bakeryLightMesh;
        [SerializeField, HideInInspector] Color color;
        [SerializeField, HideInInspector] float intensity;


        void OnValidate()
        {
            if (renderer == null) renderer = GetComponent<Renderer>();
            if (bakeryLightMesh == null) bakeryLightMesh = GetComponent<BakeryLightMesh>();
        }

        public override bool Contains(object obj)
        {
            return (obj is Renderer r && r == renderer) ||
                (obj is BakeryLightMesh m && m == bakeryLightMesh);
        }

        public override IList<ILightSource> GetLightSources()
        {
            return new List<ILightSource> { this };
        }

        public override Renderer GetSource()
        {
            return renderer;
        }

        public override void OnBakeSceneStart()
        {
            color = bakeryLightMesh.color;
            intensity = bakeryLightMesh.intensity;
            renderer.sharedMaterial.DisableKeyword(KanikamaLightMaterial.ShaderKeywordEmission);
        }

        public override void Rollback()
        {
            renderer.enabled = true;
            bakeryLightMesh.enabled = true;
            bakeryLightMesh.color = color;
            bakeryLightMesh.intensity = intensity;
        }

        public void TurnOff()
        {
            renderer.enabled = false;
            bakeryLightMesh.enabled = false;
        }

        public void OnBake()
        {
            renderer.enabled = true;
            bakeryLightMesh.enabled = true;
            bakeryLightMesh.color = Color.white;
            bakeryLightMesh.intensity = 1f;
        }
    }
}
#endif