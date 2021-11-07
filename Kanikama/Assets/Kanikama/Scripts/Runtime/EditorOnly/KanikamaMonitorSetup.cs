#if UNITY_EDITOR && !COMPILER_UDONSHARP

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Kanikama.Udon;
using VRC.Udon;
using UdonSharpEditor;
using UnityEditor;

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
            if (monitors.Count == 0) return;

            foreach (var monitor in monitors)
            {
                monitor.SetupLights(partitionType, gridRendererPrefab);
            }
            SetupCamera();
            SetupUdon();
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
            captureCamera.orthographicSize = bounds.extents.y;
        }

        void SetupUdon()
        {
            var kanikamaCamera = captureCamera.GetComponent<UdonBehaviour>();
            if (kanikamaCamera == null)
            {
                Debug.LogError($"[Kanikama] {nameof(KanikamaCamera)} component is not attached to Camera");
                return;
            }
            var type = UdonSharpEditorUtility.GetUdonSharpBehaviourType(kanikamaCamera);
            if (type != typeof(KanikamaCamera))
            {
                Debug.LogError($"[Kanikama] the type of KanikamaCamera is not {nameof(KanikamaCamera)}");
                return;
            }
            var proxy = (KanikamaCamera)UdonSharpEditorUtility.GetProxyBehaviour(kanikamaCamera);
            UdonSharpEditorUtility.CopyUdonToProxy(proxy);
            var mainMonitor = monitors[0];
            var bounds = mainMonitor.Bounds;
            var aspectRatio = bounds.size.x / bounds.size.y;
            proxy.SetAspectRatioAndPartitionType(aspectRatio, (int)partitionType);
            UdonSharpEditorUtility.CopyProxyToUdon(proxy);
            EditorUtility.SetDirty(proxy.gameObject);
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