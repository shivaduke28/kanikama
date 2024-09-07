using System.Collections.Generic;
using System.Linq;
using Kanikama.Attributes;
using Kanikama.Baking;
using UnityEngine;

namespace Kanikama.Components
{
    [RequireComponent(typeof(Renderer))]
    public class KanikamaQuadMonitorV2 : KanikamaMonitorV2
    {
        [SerializeField, NonNull] Renderer quadRenderer;
        [SerializeField, NonNull] List<LightSourceV2> gridCells;
        const float LightOffset = -0.001f;

        public override LightSourceV2 GetGridCell(int index)
        {
            return gridCells[index];
        }

        public override void SetupLights(PartitionType partitionType, LightSourceV2 gridCellPrefab)
        {
            if (quadRenderer == null) return;

            var localScale = transform.localScale;
            transform.localScale = Vector3.one;

            var children = transform.Cast<Transform>().Where(t => t.TryGetComponent<BakeTarget>(out _)).ToArray();
            foreach (var child in children)
            {
                DestroyImmediate(child.gameObject);
            }

            if (gridCells == null)
            {
                gridCells = new List<LightSourceV2>();
            }
            else
            {
                gridCells.Clear();
            }

            var bounds = GetUnRotatedBounds();

            switch (partitionType)
            {
                case PartitionType.Grid1x1:
                    SetupUniformGrid(gridCellPrefab, bounds, 1);
                    break;
                case PartitionType.Grid2x2:
                    SetupUniformGrid(gridCellPrefab, bounds, 2);
                    break;
                case PartitionType.Grid3x2:
                    SetupExpandInterior(gridCellPrefab, bounds, 3, 2);
                    break;
                case PartitionType.Grid3x3:
                    SetupExpandInterior(gridCellPrefab, bounds, 3, 3);
                    break;
                case PartitionType.Grid4x3:
                    SetupExpandInterior(gridCellPrefab, bounds, 4, 3, false, true);
                    break;
                case PartitionType.Grid4x4:
                    SetupUniformGrid(gridCellPrefab, bounds, 4);
                    break;
            }

            transform.localScale = localScale;
        }


        void OnValidate()
        {
            if (quadRenderer == null)
            {
                quadRenderer = GetComponent<Renderer>();
            }
        }

        Bounds GetUnRotatedBounds()
        {
            var t = quadRenderer.transform;
            var rotation = t.rotation;
            t.rotation = Quaternion.identity;
            var bounds = quadRenderer.bounds;
            t.rotation = rotation;
            return bounds;
        }

        void SetupUniformGrid(LightSourceV2 prefab, Bounds bounds, int count)
        {
            var size = bounds.size;
            var sizeX = size.x / count;
            var sizeY = size.y / count;
            var anchor = new Vector3(-bounds.extents.x, -bounds.extents.y, 0);

            for (var j = 0; j < count; j++)
            {
                for (var i = 0; i < count; i++)
                {
                    var item = Instantiate(prefab, transform, false);
                    var go = item.gameObject;
                    go.tag = "EditorOnly";
                    go.name = (i + j * count).ToString();
                    var t = item.transform;
                    t.localScale = new Vector3(sizeX, sizeY, 1);
                    t.localPosition = anchor + new Vector3(sizeX * (0.5f + (i % count)), sizeY * (0.5f + (j % count)), LightOffset);
                    go.SetActive(false);
                    gridCells.Add(item);
                }
            }
        }

        void SetupExpandInterior(LightSourceV2 prefab, Bounds bounds, int countX, int countY, bool expandX = true, bool expandY = true)
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
                    var item = Instantiate(prefab, transform, false);
                    var go = item.gameObject;
                    go.tag = "EditorOnly";
                    go.name = (i + j * countX).ToString();
                    var t = item.transform;
                    t.localPosition = anchor + position + new Vector3(areaX, areaY, 0) * 0.5f + new Vector3(0, 0, LightOffset);
                    t.localScale = new Vector3(areaX, areaY, 1);
                    go.SetActive(false);
                    gridCells.Add(item);
                    position += new Vector3(areaX, 0, 0);
                }

                position.y += areaY;
            }
        }
    }
}
