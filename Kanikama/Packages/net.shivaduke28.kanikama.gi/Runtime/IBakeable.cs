using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.GI
{
    public interface IBakeable
    {
        void Initialize();
        void TurnOff();
        void TurnOn();
        bool Includes(Object obj);
        void Clear();
    }

    public abstract class Bakeable : MonoBehaviour, IBakeable
    {
        public abstract void Initialize();
        public abstract void TurnOff();
        public abstract void TurnOn();
        public abstract bool Includes(Object obj);
        public abstract void Clear();
    }

}
