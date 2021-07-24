#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEngine;
using System.Collections.Generic;

namespace Kanikama.EditorOnly
{
    public class KanikamaSceneDescriptor : EditorOnlyBehaviour
    {
        [SerializeField] private List<Light> lights;
        [SerializeField] private List<Renderer> emissiveRenderers;
        [SerializeField] private List<KanikamaMonitorSetup> monitorSetups;
        [SerializeField] private bool isAmbientEnable;

        public List<Light> Lights => lights;
        public List<Renderer> EmissiveRenderers => emissiveRenderers;
        public List<KanikamaMonitorSetup> MonitorSetups => monitorSetups;
        public bool IsAmbientEnable => isAmbientEnable;
    }
}
#endif