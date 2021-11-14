﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama
{
    [RequireComponent(typeof(Renderer)), AddComponentMenu("Kanikama/KanikamaLightRenderer")]
    public class KanikamaLightRenderer : KanikamaRendererGroup
    {
        [SerializeField] Renderer renderer;

        Material[] sharedMaterials;
        Material[] tmpMaterials;
        List<KanikamaLightMaterial> lightMaterials = new List<KanikamaLightMaterial>();

        bool initialized;

        void OnValidate()
        {
            if (renderer == null) renderer = GetComponent<Renderer>();
        }

        public override Renderer GetSource()
        {
            return renderer;
        }

        public override void OnBakeSceneStart()
        {
            initialized = false;
            Initialize();
        }

        public override void Rollback()
        {
            renderer.sharedMaterials = sharedMaterials;
            lightMaterials.Clear();
        }

        public override IReadOnlyList<IKanikamaLightSource> GetLightSources()
        {
            Initialize();
            return lightMaterials.Cast<IKanikamaLightSource>().ToList().AsReadOnly();
        }

        void Initialize()
        {
            if (initialized) return;

            sharedMaterials = renderer.sharedMaterials;
            var count = sharedMaterials.Length;
            tmpMaterials = new Material[count];

            for (var i = 0; i < count; i++)
            {
                var mat = sharedMaterials[i];
                Material tmp;
                if (KanikamaLightMaterial.IsTarget(mat))
                {
                    tmp = Instantiate(mat);
                    var lightMaterial = new KanikamaLightMaterial(tmp);
                    lightMaterials.Add(lightMaterial);
                }
                else
                {
                    tmp = mat;
                }
                tmpMaterials[i] = tmp;
            }
            renderer.sharedMaterials = tmpMaterials;
            initialized = true;
        }

        public override bool Contains(object obj)
        {
            if (obj is Renderer r)
            {
                return r == renderer;
            }
            return false;
        }
    }
}