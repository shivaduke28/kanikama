#if UNITY_EDITOR && !COMPILER_UDONSHARP

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using VRC.Udon;
using UdonSharpEditor;

namespace Kanikama.EditorOnly
{
    public class KanikamaMonitorSetup : EditorOnlyBehaviour
    {
        [SerializeField] Camera captureCamera;
        [SerializeField] Transform anchor;
        [Space]
        [SerializeField] Renderer renderer;
        [SerializeField] int horizontal;
        [SerializeField] int vertical;
        [SerializeField] List<Light> lights = new List<Light>();

        public Renderer Renderer => renderer;
        public List<Light> Lights => lights;

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
            var t = renderer.transform;
            transform.SetPositionAndRotation(t.position, t.rotation);
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

            var horizontalCount = Mathf.Max(horizontal, 1f);
            var verticalCount = Mathf.Max(vertical, 1f);

            var lightSizeX = size.x / horizontalCount;
            var lightSizeY = size.y / verticalCount;

            for (var j = 0; j < verticalCount; j++)
            {
                for (var i = 0; i < horizontalCount; i++)
                {
                    var light = new GameObject((i + j * horizontalCount).ToString()).AddComponent<Light>();
                    light.type = LightType.Area;
                    light.areaSize = new Vector2(lightSizeX, lightSizeY);
                    var lightTransform = light.transform;
                    lightTransform.SetParent(anchor);
                    lightTransform.localEulerAngles = new Vector3(0, 180, 0);
                    lightTransform.localPosition = new Vector2(lightSizeX * (0.5f + (i % horizontalCount)), lightSizeY * (0.5f + (j % verticalCount)));
                    lights.Add(light);
                }
            }
        }

        [ContextMenu("Move Camera")]
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

        public void OnPreBake()
        {
            renderer.enabled = false;
            foreach (var light in lights)
            {
                light.intensity = 1;
                light.color = Color.white;
                light.enabled = false;
            }
        }

        public void OnPostBake()
        {
            renderer.enabled = true;
        }
    }
}
#endif