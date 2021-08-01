#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEngine;
using System.Collections.Generic;

namespace Kanikama.EditorOnly
{
    public class KanikamaSceneDescriptor : EditorOnlyBehaviour
    {
        [SerializeField] List<Light> lights;
        [SerializeField] List<Renderer> emissiveRenderers;
        [SerializeField] List<KanikamaMonitorSetup> monitorSetups;
        [SerializeField] bool isAmbientEnable;

        public List<Light> Lights => lights;
        public List<Renderer> EmissiveRenderers => emissiveRenderers;
        public List<KanikamaMonitorSetup> MonitorSetups => monitorSetups;
        public bool IsAmbientEnable => isAmbientEnable;
    }
}
#endif