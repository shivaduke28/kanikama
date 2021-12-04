using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama
{
    [RequireComponent(typeof(Renderer))]
    public class KanikamaUnityRenderer : KanikamaRenderer
    {
        [SerializeField] Renderer renderer;
        [SerializeField] List<KanikamaLightMaterial> lightMaterials = new List<KanikamaLightMaterial>();

        [SerializeField, HideInInspector] Material[] sharedMaterials;
        [SerializeField, HideInInspector] Material[] tmpMaterials;

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
            sharedMaterials = renderer.sharedMaterials;
            var count = sharedMaterials.Length;
            tmpMaterials = new Material[count];
            for (var i = 0; i < count; i++)
            {
                Material tmp;
                var lightMaterial = lightMaterials.FirstOrDefault(x => x.Index == i);
                if (lightMaterial != null)
                {
                    lightMaterial.OnBakeSceneStart(); // create instance
                    tmp = lightMaterial.MaterialInstance;
                }
                else
                {
                    tmp = sharedMaterials[i];
                }
                tmpMaterials[i] = tmp;
            }
            renderer.sharedMaterials = tmpMaterials;
        }

        public override void Rollback()
        {
            foreach(var mat in lightMaterials)
            {
                mat.Rollback();
            }
            renderer.sharedMaterials = sharedMaterials;
        }

        public override IList<ILightSource> GetLightSources()
        {
            return lightMaterials.Select(x => (ILightSource)x).ToList();
        }

        public void Setup()
        {
            if (lightMaterials == null) lightMaterials = new List<KanikamaLightMaterial>();

            foreach(var lightMaterial in lightMaterials)
            {
                lightMaterial.Rollback();
            }
            lightMaterials.Clear();

            sharedMaterials = renderer.sharedMaterials;
            var count = sharedMaterials.Length;

            for (var i = 0; i < count; i++)
            {
                var mat = sharedMaterials[i];
                if (KanikamaLightMaterial.IsBakedEmissive(mat))
                {
                    var lightMaterial = new KanikamaLightMaterial(i, mat);
                    lightMaterials.Add(lightMaterial);
                }
            }
        }

        public override bool Contains(object obj)
        {
            return (obj is Renderer r && r == renderer) || lightMaterials.Any(x => x.Contains(obj));
        }
    }
}