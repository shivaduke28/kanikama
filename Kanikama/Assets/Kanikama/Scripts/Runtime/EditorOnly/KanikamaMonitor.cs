#if UNITY_EDITOR && !COMPILER_UDONSHARP

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.EditorOnly
{
    public class KanikamaMonitor : EditorOnlyBehaviour
    {
        public Renderer monitorRenderer;
        [ReadOnly] public List<Renderer> gridRenderers;

        public Bounds GetUnrotatedBounds()
        {
            var rotation = monitorRenderer.transform.rotation;
            monitorRenderer.transform.rotation = Quaternion.identity;
            var bounds = monitorRenderer.bounds;
            monitorRenderer.transform.rotation = rotation;
            return bounds;
        }

        Renderer gridRendererPrefab;

        public void SetupLights(PartitionType partitionType, Renderer gridRendererPrefab)
        {
            if (monitorRenderer is null) return;
            var t = monitorRenderer.transform;
            transform.SetPositionAndRotation(t.position, t.rotation);

            this.gridRendererPrefab = gridRendererPrefab;
            var bounds = GetUnrotatedBounds();
            var children = transform.Cast<Transform>().ToArray();
            foreach (var child in children)
            {
                DestroyImmediate(child.gameObject);
            }

            gridRenderers.Clear();

            switch (partitionType)
            {
                case PartitionType.Grid1x1:
                    SetupUniformGrid(bounds, 1);
                    break;
                case PartitionType.Grid2x2:
                    SetupUniformGrid(bounds, 2);
                    break;
                case PartitionType.Grid3x2:
                    SetupExpandInterior(bounds, 3, 2);
                    break;
                case PartitionType.Grid3x3:
                    SetupExpandInterior(bounds, 3, 3);
                    break;
                case PartitionType.Grid4x3:
                    SetupExpandInterior(bounds, 4, 3, false, true);
                    break;
                case PartitionType.Grid4x4:
                    SetupUniformGrid(bounds, 4);
                    break;
            }
        }

        void SetupUniformGrid(Bounds bounds, int count)
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

        void SetupExpandInterior(Bounds bounds, int countX, int countY, bool expandX = true, bool expandY = true)
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

        public bool Contains(Renderer renderer)
        {
            return monitorRenderer == renderer || gridRenderers.Contains(renderer);
        }

        public enum PartitionType
        {
            Grid1x1 = 11,
            Grid2x2 = 22,
            Grid3x2 = 32,
            Grid3x3 = 33,
            Grid4x3 = 43,
            Grid4x4 = 44,
        }
    }
}
#endif