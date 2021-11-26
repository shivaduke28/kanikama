using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama
{
    public abstract class KanikamaMonitor : MonoBehaviour
    {
        public Renderer monitorRenderer;
        public List<KanikamaGridRenderer> gridRenderers;
        abstract public void SetupLights(PartitionType partitionType, KanikamaGridRenderer gridRendererPrefab);

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