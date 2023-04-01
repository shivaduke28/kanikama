using Kanikama.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Implements
{
    [RequireComponent(typeof(Renderer))]
    [AddComponentMenu("Kanikama/GI/KanikamaEmissiveMaterial")]
    [EditorOnly]
    public sealed class KanikamaEmissiveMaterial : LightSource
    {
        [SerializeField] new Renderer renderer;
        [SerializeField] int materialIndex;
        [SerializeField] Material original;
        [SerializeField] Material instance;

        void OnValidate()
        {
            renderer = GetComponent<Renderer>();
        }

        public override void Initialize()
        {
            gameObject.SetActive(true);
            renderer.enabled = true;
            var sharedMaterials = renderer.sharedMaterials;
            if (materialIndex < 0 || materialIndex >= sharedMaterials.Length)
            {
                return;
            }
            original = sharedMaterials[materialIndex];
            instance = Instantiate(original);
            sharedMaterials[materialIndex] = instance;
            renderer.sharedMaterials = sharedMaterials;
        }

        public override void TurnOff()
        {
            MaterialUtility.RemoveBakedEmissiveFlag(instance);
        }

        public override void TurnOn()
        {
            MaterialUtility.AddBakedEmissiveFlag(instance);
        }

        public override bool Includes(Object obj) => obj == renderer;

        public override void Clear()
        {
            var sharedMaterials = renderer.sharedMaterials;
            if (materialIndex < 0 || materialIndex >= sharedMaterials.Length)
            {
                return;
            }
            sharedMaterials[materialIndex] = original;
            renderer.sharedMaterials = sharedMaterials;
            DestroyImmediate(instance);
            instance = null;
            original = null;
        }
    }
}
