using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.Baking.Impl
{
    public class KanikamaBakeTargetMonitorGridLight : BakeTarget
    {
        [SerializeField] new Light light;

        void OnValidate()
        {
            if (light == null)
            {
                light = GetComponentInChildren<Light>();
                if (light == null)
                {
                    var go = new GameObject("Light");
                    go.transform.SetParent(transform, false);
                    go.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    light = go.AddComponent<Light>();
                    light.range = 50f;
                }
                light.type = LightType.Area;
                light.shadows = LightShadows.Soft;
            }
        }

        public override void Initialize()
        {
            var t = transform;
            var lossy = t.lossyScale;
            light.areaSize = new Vector2(lossy.x, lossy.y);
        }

        public override void TurnOff()
        {
            light.enabled = false;
        }

        public override void TurnOn()
        {
            light.color = Color.white;
            light.intensity = 1f;
        }

        public override bool Includes(Object obj) => light == obj;

        public override void Clear()
        {
        }
    }
}
