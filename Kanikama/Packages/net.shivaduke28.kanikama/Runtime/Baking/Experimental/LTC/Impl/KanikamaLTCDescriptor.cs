using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.Baking.Experimental.LTC.Impl
{
    public class KanikamaLTCDescriptor : MonoBehaviour, ILTCDescriptor
    {
        [SerializeField] List<KanikamaLTCMonitor> ltcMonitors;

        IEnumerable<LTCMonitor> ILTCDescriptor.GetMonitors() => ltcMonitors.ToArray();
    }
}
