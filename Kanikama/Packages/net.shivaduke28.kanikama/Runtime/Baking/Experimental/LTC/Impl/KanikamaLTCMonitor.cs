using UnityEngine;

namespace Kanikama.Baking.Experimental.LTC.Impl
{
    public sealed class KanikamaLTCMonitor : MonoBehaviour
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
                    go.transform.localPosition = new Vector3(0, 0, -0.001f);
                    go.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    areaLight = go.AddComponent<Light>();
                    areaLight.range = 50f;
                }
                areaLight.type = LightType.Area;
                areaLight.shadows = LightShadows.Soft;
                Initialize();
            }
        }

        public void TurnOff()
        {
            areaLight.enabled = false;
        }

        public void SetCastShadow(bool enable)
        {
            areaLight.shadows = enable ? LightShadows.Soft : LightShadows.None;
        }

        public void Initialize()
        {
            var t = transform;
            var lossy = t.lossyScale;
            areaLight.areaSize = new Vector2(lossy.x, lossy.y);
        }

        public void TurnOn()
        {
            areaLight.enabled = true;
        }
    }
}
