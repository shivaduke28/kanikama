#if UNITY_EDITOR && !COMPILER_UDONSHARP

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Kanikama.EditorOnly
{
    public class KanikamaMonitorSetup : EditorOnlyBehaviour
    {
        [SerializeField] Camera captureCamera;
        [SerializeField] Transform anchor;
        [Space]
        [SerializeField] Renderer renderer;
        [SerializeField] PartitionType partitionType;
        [SerializeField] List<Light> lights = new List<Light>();

        public List<Light> Lights => lights;
        public Renderer Renderer => renderer;

        public void Setup()
        {
            SetupTransform();
            SetupLights();
            SetupCamera();

            if (!Application.isPlaying)
            {
                var currentScene = SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(currentScene);
            }
        }

        private void SetupTransform()
        {
            if (renderer is null) return;
            var t = renderer.transform;
            transform.SetPositionAndRotation(t.position, t.rotation);
            var extents = renderer.bounds.extents;
            anchor.localPosition = new Vector3(-extents.x, -extents.y, 0);
        }

        private void SetupLights()
        {
            foreach (var l in lights)
            {
                DestroyImmediate(l.gameObject);
            }

            lights.Clear();

            switch (partitionType)
            {
                case PartitionType.Grid1x1:
                    SetupUniformGrid(1);
                    break;
                case PartitionType.Grid2x2:
                    SetupUniformGrid(2);
                    break;
                case PartitionType.Grid2x3:
                    SetupExpandInterior(2, 3);
                    break;
                case PartitionType.Grid3x3:
                    SetupExpandInterior(3, 3);
                    break;
                case PartitionType.Grid3x4:
                    SetupExpandInterior(3, 4, true, false);
                    break;
                case PartitionType.Grid4x4:
                    SetupUniformGrid(4);
                    return;
            }
        }

        private void SetupUniformGrid(int count)
        {
            var size = renderer.bounds.size;
            var sizeX = size.x / count;
            var sizeY = size.y / count;

            for (var j = 0; j < count; j++)
            {
                for (var i = 0; i < count; i++)
                {
                    var light = new GameObject((i + j * count).ToString()).AddComponent<Light>();
                    light.type = LightType.Area;
                    light.areaSize = new Vector2(sizeX, sizeY);
                    var lightTransform = light.transform;
                    lightTransform.SetParent(anchor);
                    lightTransform.localEulerAngles = new Vector3(0, 180, 0);
                    lightTransform.localPosition = new Vector2(sizeX * (0.5f + (i % count)), sizeY * (0.5f + (j % count)));
                    lights.Add(light);
                }
            }
        }

        private void SetupExpandInterior(int countY, int countX, bool expandY = true, bool expandX = true)
        {
            var size = renderer.bounds.size;

            var sizeX = size.x / (countX + (countX % 2));
            var sizeY = size.y / (countY + (countY % 2));

            var position = Vector2.zero;
            for (var j = 0; j < countY; j++)
            {
                position.x = 0;
                var areaY = !expandY || (j == 0 || j == countY - 1) ? sizeY : sizeY * 2;
                for (var i = 0; i < countX; i++)
                {
                    var light = new GameObject((i + j * countX).ToString()).AddComponent<Light>();
                    light.type = LightType.Area;
                    var areaX = !expandX || (i == 0 || i == countX - 1) ? sizeX : sizeX * 2;
                    light.areaSize = new Vector2(areaX, areaY);
                    var lightTransform = light.transform;
                    lightTransform.SetParent(anchor);
                    lightTransform.localEulerAngles = new Vector3(0, 180, 0);
                    lightTransform.localPosition = position + new Vector2(areaX, areaY) * 0.5f;
                    lights.Add(light);
                    position += new Vector2(areaX, 0);
                }

                position.y += areaY;
            }
        }

        private void SetupCamera()
        {
            var cameraTrans = captureCamera.transform;
            var rendererTrans = renderer.transform;
            cameraTrans.SetPositionAndRotation(rendererTrans.position - rendererTrans.forward * 0.01f, rendererTrans.rotation);
            captureCamera.nearClipPlane = 0;
            captureCamera.farClipPlane = 0.02f;
            captureCamera.orthographicSize = Mathf.Min(renderer.bounds.extents.x, renderer.bounds.extents.y);

        }

        public void TurnOff()
        {
            renderer.enabled = false;
            foreach (var light in lights)
            {
                light.intensity = 1;
                light.color = Color.white;
                light.enabled = false;
            }
        }

        public void RollBack()
        {
            renderer.enabled = true;
        }

        private enum PartitionType
        {
            Grid1x1 = 11,
            Grid2x2 = 22,
            Grid2x3 = 23,
            Grid3x3 = 33,
            Grid3x4 = 34,
            Grid4x4 = 44,
        }
    }
}
#endif