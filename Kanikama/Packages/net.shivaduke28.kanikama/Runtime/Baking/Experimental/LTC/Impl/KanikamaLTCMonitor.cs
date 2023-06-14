using UnityEngine;

namespace Kanikama.Baking.Experimental.LTC.Impl
{
    public sealed class KanikamaLTCMonitor : LTCMonitor
    {
        [SerializeField] Light areaLight;

        void OnValidate()
        {
            if (areaLight == null)
            {
                areaLight = GetComponentInChildren<Light>();
                if (areaLight == null)
                {
                    var go = new GameObject("Light");
                    go.transform.SetParent(transform, false);
                    go.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    areaLight = go.AddComponent<Light>();
                    areaLight.range = 50f;
                }
                areaLight.type = LightType.Area;
                areaLight.shadows = LightShadows.Soft;
                Initialize();
            }
        }

        public override void TurnOff()
        {
            gameObject.SetActive(false);
            areaLight.enabled = false;
        }

        public override void SetCastShadow(bool enable)
        {
            areaLight.shadows = enable ? LightShadows.Soft : LightShadows.None;
        }

        public override bool Includes(Object obj) => obj is Light l && l == areaLight;

        public override void Initialize()
        {
            var t = transform;
            var lossy = t.lossyScale;
            areaLight.areaSize = new Vector2(lossy.x, lossy.y);
            gameObject.SetActive(false);
        }

        public override void TurnOn()
        {
            gameObject.SetActive(true);
            areaLight.enabled = true;
        }
    }
}
