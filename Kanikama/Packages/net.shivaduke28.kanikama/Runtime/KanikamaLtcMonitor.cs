namespace Kanikama
{
    public abstract class KanikamaLtcMonitor : KanikamaBehaviour
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        , IKanikamaBakeTarget
#endif
    {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public abstract void Initialize();
        public abstract void TurnOn();
        public abstract void TurnOff();
        public abstract void Clear();
#endif
    }
}
