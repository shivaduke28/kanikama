using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama
{
    public abstract class KanikamaLightSourceGroup : MonoBehaviour, IKanikamaLightSourceGroup
    {
        abstract public void OnBakeSceneStart();
        abstract public void Rollback();
        abstract public IReadOnlyList<IKanikamaLightSource> GetLightSources();
        abstract public bool Contains(object obj);
    }

    public abstract class KanikamaLightSourceGroup<T> : KanikamaLightSourceGroup
    {
        abstract public T GetSource();
    }
}