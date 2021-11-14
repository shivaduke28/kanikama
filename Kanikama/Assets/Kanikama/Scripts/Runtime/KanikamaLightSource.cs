using System;
using UnityEngine;

namespace Kanikama
{
    public abstract class KanikamaLightSource : MonoBehaviour, IKanikamaLightSource
    {
        abstract public void OnBake();
        abstract public void Rollback();
        abstract public void TurnOff();
        abstract public bool Contains(object obj);
    }

    public abstract class KanikamaLightSource<T> : MonoBehaviour, IKanikamaLightSource
    {
        abstract public T GetSource();
        abstract public void OnBake();
        abstract public void Rollback();
        abstract public void TurnOff();
        abstract public bool Contains(object obj);
    }

}