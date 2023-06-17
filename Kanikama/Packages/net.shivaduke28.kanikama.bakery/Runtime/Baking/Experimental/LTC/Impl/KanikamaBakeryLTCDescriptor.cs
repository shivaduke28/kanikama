using System.Collections.Generic;
using UnityEngine;

namespace Baking.Experimental.LTC.Impl
{
    public sealed class KanikamaBakeryLTCDescriptor : MonoBehaviour
    {
        [SerializeField] List<KanikamaBakeryLTCMonitor> monitors;
        public KanikamaBakeryLTCMonitor[] GetMonitors() => monitors.ToArray();
    }
}
