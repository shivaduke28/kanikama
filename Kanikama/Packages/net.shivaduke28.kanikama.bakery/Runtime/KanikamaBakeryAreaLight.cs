using UnityEngine;

namespace Kanikama.Bakery
{
    [RequireComponent(typeof(Light), typeof(BakeryLightMesh))]
    public sealed class KanikamaBakeryAreaLight : KanikamaLightSource
    {
        [SerializeField] new Light light;
        [SerializeField, HideInInspector] float intensity;
        [SerializeField, HideInInspector] Color color;
        [SerializeField, HideInInspector] bool lightEnabled;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        BakeryLightMesh BakeryLight => GetComponent<BakeryLightMesh>();

        void Reset()
        {
            light = GetComponent<Light>();
        }

        public override void Initialize()
        {
            intensity = BakeryLight.intensity;
            color = BakeryLight.color;
            lightEnabled = light.enabled;
        }

        public override void TurnOff()
        {
            light.enabled = false;
            BakeryLight.enabled = false;
        }

        public override void TurnOn()
        {
            BakeryLight.enabled = true;
            BakeryLight.color = Color.white;
            BakeryLight.intensity = 1f;
            light.color = Color.white;
            light.intensity = 1f;
            light.enabled = true;
        }

        public override void Clear()
        {
            BakeryLight.color = color;
            BakeryLight.intensity = intensity;
            BakeryLight.enabled = true;
            light.color = color;
            light.intensity = intensity;
            light.enabled = lightEnabled;
        }
#endif

        public override Color GetLinearColor()
        {
            return light.color.linear * light.intensity;
        }
    }
}
