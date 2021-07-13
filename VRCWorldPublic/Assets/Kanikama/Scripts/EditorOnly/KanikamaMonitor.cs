#if UNITY_EDITOR && !COMPILER_UDONSHARP

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Kanikama.EditorOnly
{
    public class KanikamaMonitor : EditorOnlyBehaviour
    {
        public MeshRenderer renderer;
        public Vector3 center;
        public Vector3 size;
        public Vector3 min;
        public Vector3 max;
        public Vector3 extents;

        public Transform anchor;

        public int horizontal;
        public int vertical;
        public List<Light> lights = new List<Light>();

        private void OnEnable()
        {
        }

        [ContextMenu("update lights")]
        void UpdateLights()
        {
            UpdateBounds();
            CreateLights();
        }

        void UpdateBounds()
        {
            if (renderer is null) return;
            var b = renderer.bounds;
            center = b.center;
            size = b.size;
            min = b.min;
            max = b.max;
            extents = b.extents;

            transform.position = renderer.transform.position;
            transform.rotation = renderer.transform.rotation;
            anchor.position = min;
        }

        void CreateLights()
        {
            foreach (var l in lights)
            {
                DestroyImmediate(l.gameObject);
            }

            lights.Clear();

            var width = size.x;
            var height = size.y;

            var x = Mathf.Max((float)horizontal, 1f);
            var y = Mathf.Max((float)vertical, 1f);

            var sx = width / x;
            var sy = height / y;

            for (var j = 0; j < y; j++)
            {
                for (var i = 0; i < x; i++)
                {
                    var l = new GameObject((i + j * x).ToString()).AddComponent<Light>();
                    l.type = LightType.Area;
                    l.areaSize = new Vector2(sx, sy);
                    l.transform.SetParent(anchor);
                    l.transform.localEulerAngles = new Vector3(0, 180, 0);
                    l.transform.localPosition = new Vector2(sx * (0.5f + (i % x)), sy * (0.5f + (j % y)));
                    lights.Add(l);
                }
            }

        }


    }
}
#endif