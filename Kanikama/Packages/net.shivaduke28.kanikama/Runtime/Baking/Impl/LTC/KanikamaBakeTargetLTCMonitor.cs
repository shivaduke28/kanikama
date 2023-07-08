using Kanikama.Baking.Attributes;
using Kanikama.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.Baking.Impl.LTC
{
    // Attach this to a Renderer with Unity Quad mesh.
    [DisallowMultipleComponent]
    public sealed class KanikamaBakeTargetLTCMonitor : LTCMonitor
    {
        // Because Unity can not bake lightmaps w/o shadows for emissive Renderers,
        // we use Area Light here.
        [SerializeField, NonNull] Light areaLight;

        void OnValidate()
        {
            if (areaLight == null)
            {
                Reset();
            }
        }

        void Reset()
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
            else
            {
                areaLight = child.GetComponent<Light>();
            }
            areaLight.type = LightType.Area;
            areaLight.shadows = LightShadows.Soft;
#if UNITY_EDITOR
            // NOTE: Light.areaSize is editor only.
            var t = transform;
            var lossy = t.lossyScale;
            areaLight.areaSize = new Vector2(lossy.x, lossy.y);
#endif
            areaLight.gameObject.SetActive(false);
        }

        public override void Initialize()
        {
            areaLight.gameObject.SetActive(true);
        }

        public override void TurnOff()
        {
            areaLight.enabled = false;
        }

        public override void TurnOn()
        {
            areaLight.enabled = true;
            SelectionUtility.SetActiveObject(areaLight);
        }

        public override bool Includes(Object obj) => obj == areaLight;

        public override void Clear()
        {
            areaLight.gameObject.SetActive(false);
        }
    }
}
