#if UNITY_EDITOR && !COMPILER_UDONSHARP

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.EditorOnly
{
    public class KanikamaMonitorSetup : EditorOnlyBehaviour
    {
        [SerializeField] Camera captureCamera;
        [SerializeField] Renderer gridRendererPrefab;
        [Space]
        [SerializeField] Renderer monitorRenderer;
        [SerializeField] PartitionType partitionType;
        [SerializeField] CameraDetailedSettings cameraDetailedSettings;
        [SerializeField] List<Renderer> gridRenderers;

        public List<Renderer> GridRenderers => gridRenderers;
        public Renderer Renderer => monitorRenderer;

        Bounds bounds;

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
        }

        void SetupLights()
        {
            var rotation = monitorRenderer.transform.rotation;
            monitorRenderer.transform.rotation = Quaternion.identity;
            bounds = monitorRenderer.bounds;
            monitorRenderer.transform.rotation = rotation;
            var children = transform.Cast<Transform>().ToArray();
            foreach (var child in children)
            {
                DestroyImmediate(child.gameObject);
            }

            gridRenderers.Clear();

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
            var size = bounds.size;
            var sizeX = size.x / count;
            var sizeY = size.y / count;
            var anchor = new Vector3(-bounds.extents.x, -bounds.extents.y, 0);

            for (var j = 0; j < count; j++)
            {
                for (var i = 0; i < count; i++)
                {
                    var item = Instantiate(gridRendererPrefab, transform, false);
                    item.gameObject.name = (i + j * count).ToString();
                    item.transform.localScale = new Vector3(sizeX, sizeY, 1);
                    item.transform.localPosition = anchor + new Vector3(sizeX * (0.5f + (i % count)), sizeY * (0.5f + (j % count)), 0);
                    item.enabled = false;
                    gridRenderers.Add(item);
                }
            }
        }

        void SetupExpandInterior(int countX, int countY, bool expandX = true, bool expandY = true)
        {
            var size = bounds.size;

            var sizeX = size.x / (countX + (countX % 2));
            var sizeY = size.y / (countY + (countY % 2));
            var anchor = new Vector3(-bounds.extents.x, -bounds.extents.y, 0);

            var position = Vector3.zero;
            for (var j = 0; j < countY; j++)
            {
                position.x = 0;
                var areaY = !expandY || (j == 0 || j == countY - 1) ? sizeY : sizeY * 2;
                for (var i = 0; i < countX; i++)
                {
                    var areaX = !expandX || (i == 0 || i == countX - 1) ? sizeX : sizeX * 2;
                    var item = Instantiate(gridRendererPrefab, transform, false);
                    item.gameObject.name = (i + j * countX).ToString();
                    item.transform.localScale = new Vector3(sizeX, sizeY, 1);
                    item.transform.localPosition = anchor + position + new Vector3(areaX, areaY, 0) * 0.5f;
                    item.transform.localScale = new Vector3(areaX, areaY, 1);
                    item.enabled = false;
                    gridRenderers.Add(item);
                    position += new Vector3(areaX, 0, 0);
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
            captureCamera.orthographicSize = Mathf.Min(bounds.extents.x, bounds.extents.y);

        }

        public void TurnOff()
        {
            monitorRenderer.enabled = false;
            foreach (var renderer in gridRenderers)
            {
                renderer.enabled = false;
            }
        }

        public void OnBake()
        {
            foreach (var renderer in gridRenderers)
            {
                renderer.enabled = true;
            }
        }

        public void RollBack()
        {
            monitorRenderer.enabled = true;
            foreach (var renderer in gridRenderers)
            {
                renderer.enabled = false;
            }
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