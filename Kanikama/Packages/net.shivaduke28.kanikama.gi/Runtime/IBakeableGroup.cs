using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.GI
{
    public interface IBakeableGroup
    {
        List<IBakeable> GetAll();
        IBakeable Get(int index);
    }

    public abstract class BakeableGroup : MonoBehaviour, IBakeableGroup
    {
        public abstract List<IBakeable> GetAll();
        public abstract IBakeable Get(int index);
    }
}
