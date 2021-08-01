#if UNITY_EDITOR && !COMPILER_UDONSHARP

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Kanikama.EditorOnly
{
    public class KanikamaMonitorSetup : EditorOnlyBehaviour
    {
        [SerializeField] Camera captureCamera;
        [SerializeField] Transform anchor;
        [Space]
        [SerializeField] Renderer monitorRenderer;
        [SerializeField] PartitionType partitionType;
        [SerializeField] CameraDetailedSettings cameraDetailedSettings;
        [SerializeField] List<Light> lights = new List<Light>();

        public List<Light> Lights => lights;
        public Renderer Renderer => monitorRenderer;

        public void Setup()
        {
            SetupTransform();
            SetupLights();
            SetupCamera();
        }

        void SetupTransform()
        {
            if (monitorRenderer is null) return;
            var t = monitorRenderer.transform;
            transform.SetPositionAndRotation(t.position, t.rotation);
            var extents = monitorRenderer.bounds.extents;
            anchor.localPosition = new Vector3(-extents.x, -extents.y, 0);
        }

        void SetupLights()
        {
            var children = anchor.Cast<Transform>().ToArray();
            foreach (var child in children)
            {
                DestroyImmediate(child.gameObject);
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
                case PartitionType.Grid3x2:
                    SetupExpandInterior(3, 2);
                    break;
                case PartitionType.Grid3x3:
                    SetupExpandInterior(3, 3);
                    break;
                case PartitionType.Grid4x3:
                    SetupExpandInterior(4, 3, false, true);
                    break;
                case PartitionType.Grid4x4:
                    SetupUniformGrid(4);
                    return;
            }
        }

        void SetupUniformGrid(int count)
        {
            var size = monitorRenderer.bounds.size;
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

        void SetupExpandInterior(int countX, int countY, bool expandX = true, bool expandY = true)
        {
            var size = monitorRenderer.bounds.size;

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

        void SetupCamera()
        {
            var cameraTrans = captureCamera.transform;
            var rendererTrans = monitorRenderer.transform;
            cameraTrans.SetPositionAndRotation(rendererTrans.position - rendererTrans.forward * cameraDetailedSettings.distance, rendererTrans.rotation);
            captureCamera.nearClipPlane = cameraDetailedSettings.near;
            captureCamera.farClipPlane = cameraDetailedSettings.far;
            captureCamera.orthographicSize = Mathf.Min(monitorRenderer.bounds.extents.x, monitorRenderer.bounds.extents.y);

        }

        public void TurnOff()
        {
            monitorRenderer.enabled = false;
            foreach (var light in lights)
            {
                light.intensity = 1;
                light.color = Color.white;
                light.enabled = false;
            }
        }

        public void RollBack()
        {
            monitorRenderer.enabled = true;
        }

        enum PartitionType
        {
            Grid1x1 = 11,
            Grid2x2 = 22,
            Grid3x2 = 32,
            Grid3x3 = 33,
            Grid4x3 = 43,
            Grid4x4 = 44,
        }

        [Serializable]
        class CameraDetailedSettings
        {
            public float near = 0f;
            public float far = 0.2f;
            public float distance = 0.1f;
        }
    }
}
#endif