using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.GI
{
    // TODO: implement BakeableGroup
    public abstract class LightSourceGroup : MonoBehaviour
    {
        public abstract IList<IBakeable> GetLightSources();

        public bool Includes(Object obj)
        {
            return false;
        }
    }
}
