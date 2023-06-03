using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.Baking
{
    public interface IBakeTargetGroup
    {
        List<IBakeTarget> GetAll();
        IBakeTarget Get(int index);
    }

    public abstract class BakeTargetGroup : MonoBehaviour, IBakeTargetGroup
    {
        public abstract List<IBakeTarget> GetAll();
        public abstract IBakeTarget Get(int index);
    }
}
