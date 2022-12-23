using UnityEngine;
using System.Collections.Generic;

namespace Kanikama
{
    public interface ILightSourceGroup
    {
        void OnBakeSceneStart();
        void Rollback();
        bool Contains(object obj); // should call lightSources.Any(x => x.Contains(obj))
        IList<ILightSource> GetLightSources();
    }

    public abstract class KanikamaLightSourceGroup : MonoBehaviour, ILightSourceGroup
    {
        abstract public void OnBakeSceneStart();
        abstract public void Rollback();
        abstract public IList<ILightSource> GetLightSources();
        abstract public bool Contains(object obj);
    }

    public abstract class KanikamaLightSourceGroup<T> : KanikamaLightSourceGroup
    {
        abstract public T GetSource();
    }
}