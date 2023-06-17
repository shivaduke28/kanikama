using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.Baking.Experimental.LTC.Impl
{
    public class KanikamaLTCDescriptor : MonoBehaviour
    {
        [SerializeField] List<KanikamaLTCMonitor> ltcMonitors;
        public KanikamaLTCMonitor[] GetMonitors() => ltcMonitors.ToArray();
    }
}
