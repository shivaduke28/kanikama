using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.GI
{
    public abstract class LightSourceGroup : MonoBehaviour
    {
        public abstract IEnumerable<ILightSourceHandle> GetHandles();
    }
}
