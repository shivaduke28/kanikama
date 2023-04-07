using System;
using UnityEngine;

namespace Kanikama.GI.Runtime.Impl
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Kanikama/GI/Runtime/KanikamaMonitorCamera")]
    public sealed class KanikamaMonitorCamera : LightSourceGroup
    {
        [SerializeField] CameraSettings cameraSettings;

        [Serializable]
        class CameraSettings
        {
            public float near = 0f;
            public float far = 0.2f;
            public float distance = 0.1f;
        }

        void OnPostRender()
        {
            // TODO: 色...
        }
    }
}
