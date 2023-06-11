using System.Collections.Generic;
using Kanikama.Baking.Impl;
using UnityEngine;

namespace Kanikama.Baking.LTC.Impl
{
    public class KanikamaLTCDescriptor : MonoBehaviour, ILTCDescriptor
    {
        [SerializeField] KanikamaBakeTargetMonitorGroup monitorGroup;
        [SerializeField] List<KanikamaBakeTargetMonitor> monitors;

        BakeTargetGroup ILTCDescriptor.GetMonitorGroup() => monitorGroup;
        IEnumerable<BakeTargetGroup> ILTCDescriptor.GetMonitors() => monitors.ToArray();
    }
}
