#if UNITY_EDITOR && !COMPILER_UDONSHARP

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KanikamaGI.EditorOnly
{
    public class KanikamaLightReference : MonoBehaviour
    {
        public List<Light> lights;
    }
}
#endif