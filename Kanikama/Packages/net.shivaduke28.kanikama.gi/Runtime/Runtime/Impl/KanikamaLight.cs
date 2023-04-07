using Kanikama.Core;
using UnityEngine;

namespace Kanikama.GI.Runtime.Impl
{
    [RequireComponent(typeof(Light))]
    [AddComponentMenu("Kanikama/GI/Runtime/KanikamaLight")]
    [EditorOnly]
    public sealed class KanikamaLight : LightSource
    {
        [SerializeField] new Light light;

        void OnValidate()
        {
            light = GetComponent<Light>();
        }

        public override Color GetColorLinear()
        {
            return light.color.linear * light.intensity;
        }
    }
}
