﻿using Kanikama.Baking.Impl.LTC;
using UnityEngine;
using UnityEngine.Rendering;

namespace Baking.Experimental.LTC.Impl
{
    [RequireComponent(typeof(BakeryLightMesh))]
    [RequireComponent(typeof(Renderer))]
    public sealed class KanikamaBakeryLTCMonitor : KanikamaLTCMonitor
    {
        [SerializeField] BakeryLightMesh bakeryLightMesh;
        [SerializeField] new Renderer renderer;

        void OnValidate() => Initialize();

        public override void Initialize()
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

        public override void TurnOn()
        {
            renderer.shadowCastingMode = ShadowCastingMode.On;
            bakeryLightMesh.selfShadow = true;
            bakeryLightMesh.enabled = true;
        }

        public override void TurnOff()
        {
            bakeryLightMesh.enabled = false;
        }
    }
}
