﻿using UnityEngine;

namespace Kanikama
{
    public abstract class KanikamaMonitorV2 : MonoBehaviour
    {
        public abstract KanikamaLightSource GetLightSource(int index);
        public abstract void SetupLights(KanikamaMonitorPartitionType partitionType, KanikamaLightSource prefab);
    }


    public enum KanikamaMonitorPartitionType
    {
        Grid1x1 = 11,
        Grid2x2 = 22,
        Grid3x2 = 32,
        Grid3x3 = 33,
        Grid4x3 = 43,
        Grid4x4 = 44,
    }
}
