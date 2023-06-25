﻿using Kanikama.Baking;
using UnityEngine;

namespace Kanikama.Bakery.Baking.Impl
{
    [RequireComponent(typeof(Renderer), typeof(BakeryLightMesh))]
    public sealed class KanikamaBakeryLightMesh : BakeTarget
    {
        [SerializeField] new Renderer renderer;
        [SerializeField] BakeryLightMesh bakeryLightMesh;
        [SerializeField] string gameObjectTag;
        [SerializeField] Color color;
        [SerializeField] float intensity;
        [SerializeField] bool rendererEnable;
        [SerializeField] bool bakeryLightMeshEnable;

        void OnValidate()
        {
            if (renderer == null) renderer = GetComponent<Renderer>();
            if (bakeryLightMesh == null) bakeryLightMesh = GetComponent<BakeryLightMesh>();
        }

        public override void Initialize()
        {
            color = bakeryLightMesh.color;
            intensity = bakeryLightMesh.intensity;
            gameObjectTag = gameObject.tag;
            gameObject.tag = "Untagged";
            rendererEnable = renderer.enabled;
            bakeryLightMeshEnable = bakeryLightMesh.enabled;
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
            renderer.enabled = rendererEnable;
            bakeryLightMesh.enabled = bakeryLightMeshEnable;
            bakeryLightMesh.color = color;
            bakeryLightMesh.intensity = intensity;
            gameObject.tag = gameObjectTag;
        }
    }
}
