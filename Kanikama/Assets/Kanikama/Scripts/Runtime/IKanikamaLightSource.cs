using System.Collections;
using UnityEngine;

namespace Kanikama
{
    public interface IKanikamaLightSource
    {
        void TurnOff();
        void OnBake();
        void Rollback();
        bool Contains(object obj);
    }
}