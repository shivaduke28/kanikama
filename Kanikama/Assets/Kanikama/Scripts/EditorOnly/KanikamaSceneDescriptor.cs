#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Kanikama.EditorOnly
{
    public class KanikamaSceneDescriptor : EditorOnlyBehaviour
    {
        public List<Light> kanikamaLights;
        public List<KanikamaMonitorSetup> kanikamaMonitors;
    }
}
#endif