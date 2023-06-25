using UnityEngine;

namespace Kanikama.Baking.Impl
{
    public abstract class KanikamaMonitor : MonoBehaviour
    {
        public abstract BakeTarget GetBakeTarget(int index);
        public abstract void SetupLights(PartitionType partitionType, BakeTarget prefab);

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
