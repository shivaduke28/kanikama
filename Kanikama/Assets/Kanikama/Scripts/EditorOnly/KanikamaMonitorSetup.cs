#if UNITY_EDITOR && !COMPILER_UDONSHARP

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.EditorOnly
{
    public class KanikamaMonitorSetup : EditorOnlyBehaviour
    {
        [SerializeField] Camera captureCamera;
        [SerializeField] Renderer gridRendererPrefab;
        [Space]
        [SerializeField] List<KanikamaMonitor> monitors;
        [SerializeField] KanikamaMonitor.PartitionType partitionType;
        [SerializeField] CameraDetailedSettings cameraDetailedSettings;

        public List<KanikamaMonitor> Monitors => monitors;
        public KanikamaMonitor MainMonitor => monitors.FirstOrDefault();

        public void Setup()
        {
            if (!monitors.Any()) return;

            foreach (var monitor in monitors)
            {
                monitor.SetupLights(partitionType, gridRendererPrefab);
            }
            SetupCamera();
        }

        void SetupCamera()
        {
            var mainMonitor = monitors[0];
            var cameraTrans = captureCamera.transform;
            var rendererTrans = mainMonitor.transform;
            cameraTrans.SetPositionAndRotation(rendererTrans.position - rendererTrans.forward * cameraDetailedSettings.distance, rendererTrans.rotation);
            captureCamera.nearClipPlane = cameraDetailedSettings.near;
            captureCamera.farClipPlane = cameraDetailedSettings.far;
            var bounds = mainMonitor.Bounds;
            captureCamera.orthographicSize = Mathf.Min(bounds.extents.x, bounds.extents.y);
        }

        public void TurnOff()
        {
            foreach (var monitor in monitors)
            {
                monitor.TurnOff();
            }
        }

        public void OnBake()
        {
            foreach (var monitor in monitors)
            {
                monitor.OnBake();
            }
        }

        public void RollBack()
        {
            foreach (var monitor in monitors)
            {
                monitor.RollBack();
            }
        }

        public bool Contains(Renderer renderer)
        {
            return monitors.Any(x => x.Contains(renderer));
        }

        [Serializable]
        class CameraDetailedSettings
        {
            public float near = 0f;
            public float far = 0.2f;
            public float distance = 0.1f;
        }
    }
}
#endif