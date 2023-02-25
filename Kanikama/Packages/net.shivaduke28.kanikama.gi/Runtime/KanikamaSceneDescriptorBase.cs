using UnityEngine;

namespace Kanikama.GI
{
    public abstract class KanikamaSceneDescriptorBase : MonoBehaviour
    {
        public abstract ILightSourceHandle[] GetLightSources();
    }
}