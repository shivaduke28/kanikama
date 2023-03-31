using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.GI
{
    public abstract class LightSourceGroup : MonoBehaviour
    {
        public abstract IList<ILightSource> GetLightSources();

        public bool Includes(Object obj)
        {
            return GetLightSources().Any(l => l.Includes(obj));
        }
    }
}
