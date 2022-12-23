using System.Collections;
using UnityEngine;

namespace Kanikama
{
    public interface ILightSource
    {
        void OnBakeSceneStart();
        void TurnOff();
        void OnBake();
        void Rollback();
        bool Contains(object obj);
    }

    public abstract class LightSource : MonoBehaviour, ILightSource
    {
        abstract public void OnBakeSceneStart();
        abstract public void OnBake();
        abstract public void Rollback();
        abstract public void TurnOff();
        abstract public bool Contains(object obj);
    }

    public abstract class LightSource<T> : LightSource
    {
        abstract public T GetSource();
    }
}