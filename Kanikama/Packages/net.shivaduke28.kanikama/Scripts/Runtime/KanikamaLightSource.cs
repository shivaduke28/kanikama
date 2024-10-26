using System.Collections.Generic;
using Kanikama;
using UnityEngine;

namespace Kanikama
{
    public interface IKanikamaBakeTarget
    {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        void Initialize();
        void TurnOff();
        void TurnOn();
        void Clear();
#endif
    }

    public abstract class KanikamaLightSource : KanikamaBehaviour
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        , IKanikamaBakeTarget
#endif
    {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public abstract void Initialize();
        public abstract void TurnOff();
        public abstract void TurnOn();
        public abstract void Clear();
#endif
        public abstract Color GetLinearColor();
    }

    public abstract class KanikamaLightSourceGroup : KanikamaBehaviour
    {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public abstract List<IKanikamaBakeTarget> GetAll();
        public abstract IKanikamaBakeTarget Get(int index);
#endif
        public abstract Color[] GetLinearColors();
    }
}
