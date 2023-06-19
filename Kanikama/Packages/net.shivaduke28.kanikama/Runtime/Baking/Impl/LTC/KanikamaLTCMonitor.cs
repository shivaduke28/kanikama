using UnityEngine;

namespace Kanikama.Baking.Impl.LTC
{
    public sealed class KanikamaLTCMonitor : MonoBehaviour
    {
        [SerializeField] Light areaLight;

        void OnValidate()
        {
            if (areaLight == null)
            {
                var child = transform.Find("LTCLight");
                if (child == null)
                {
                    var go = new GameObject("LTCLight");
                    go.transform.SetParent(transform, false);
                    go.transform.localPosition = new Vector3(0, 0, -0.001f);
                    go.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    areaLight = go.AddComponent<Light>();
                    areaLight.range = 50f;
                }
                areaLight.type = LightType.Area;
                areaLight.shadows = LightShadows.Soft;
                areaLight.enabled = false;
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
#if UNITY_EDITOR
            // NOTE: Light.areaSize is editor only.
            var t = transform;
            var lossy = t.lossyScale;
            areaLight.areaSize = new Vector2(lossy.x, lossy.y);
#endif
        }

        public void TurnOn()
        {
            areaLight.enabled = true;
        }
    }
}
