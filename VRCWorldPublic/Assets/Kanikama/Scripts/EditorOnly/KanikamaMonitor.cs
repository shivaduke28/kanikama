#if UNITY_EDITOR && !COMPILER_UDONSHARP

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Kanikama.EditorOnly
{
    public class KanikamaMonitor : EditorOnlyBehaviour
    {
        public MeshRenderer renderer;
        public Transform anchor;
        public int horizontal;
        public int vertical;
        public List<Light> lights = new List<Light>();
        public Camera captureCamera;

        [ContextMenu("Update Lights and Camera")]
        void UpdateLights()
        {
            UpdateBounds();
            CreateLights();
            MoveCamera();

            var currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(currentScene);
        }

        void UpdateBounds()
        {
            if (renderer is null) return;
            transform.SetPositionAndRotation(renderer.transform.position, renderer.transform.rotation);
            anchor.position = renderer.bounds.min;
        }

        void CreateLights()
        {
            foreach (var l in lights)
            {
                DestroyImmediate(l.gameObject);
            }

            lights.Clear();

            var size = renderer.bounds.size;
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

        [ContextMenu("UpdateCamere")]
        void MoveCamera()
        {
            var cameraTrans = captureCamera.transform;
            var rendererTrans = renderer.transform;
            cameraTrans.SetPositionAndRotation(rendererTrans.position - rendererTrans.forward * 0.01f, rendererTrans.rotation);
            captureCamera.nearClipPlane = 0;
            captureCamera.farClipPlane = 0.02f;
            (captureCamera.orthographicSize, captureCamera.rect) = CalculateCameraSizeAndRect(renderer.bounds.extents);
        }

        static (float, Rect) CalculateCameraSizeAndRect(Vector3 extents)
        {
            var x = extents.x;
            var y = extents.y;

            if (x >= y)
            {
                return (y, new Rect { width = 1, height = y / x });
            }
            else
            {
                return (x, new Rect { width = y / x, height = 1 });
            }
        }
    }
}
#endif