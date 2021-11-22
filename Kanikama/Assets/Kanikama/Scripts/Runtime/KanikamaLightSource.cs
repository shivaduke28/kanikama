using System;
using UnityEngine;

namespace Kanikama
{
    public abstract class KanikamaLightSource : MonoBehaviour, IKanikamaLightSource
    {
        abstract public void OnBakeSceneStart();
        abstract public void OnBake();
        abstract public void Rollback();
        abstract public void TurnOff();
        abstract public bool Contains(object obj);
    }

    public abstract class KanikamaLightSource<T> : KanikamaLightSource
    {
        abstract public T GetSource();
    }

}