using System;
using System.Collections.Generic;
using System.Linq;
using Kanikama.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Implements
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Kanikama/GI/KanikamaMonitorCamera")]
    [EditorOnly]
    public sealed class KanikamaMonitorCamera : LightSourceGroup
    {
        [SerializeField] new Camera camera;
        [SerializeField] KanikamaMonitor mainMonitor;
        [SerializeField] KanikamaMonitor[] subMonitors;
        [SerializeField] LightSource lightSourcePrefab;
        [SerializeField] KanikamaMonitor.PartitionType partitionType;
        [SerializeField] CameraSettings cameraSettings;
        [SerializeField] List<MonitorGridFiber> monitorGridFibers;


        public Camera Camera => camera;
        public KanikamaMonitor.PartitionType PartitionType => partitionType;
        public KanikamaMonitor MainMonitor => mainMonitor;

        void OnValidate()
        {
            if (camera == null) camera = GetComponent<Camera>();
        }

        public override IList<ILightSource> GetLightSources()
        {
            return new List<ILightSource>(monitorGridFibers);
        }


        public void Setup()
        {
            if (mainMonitor == null) return;
            mainMonitor.SetupLights(partitionType, lightSourcePrefab);
            foreach (var monitor in subMonitors)
            {
                monitor.SetupLights(partitionType, lightSourcePrefab);
            }
            SetupCamera();
            SetupMonitorGridFibers();
        }

        void SetupCamera()
        {
            var cameraTrans = camera.transform;
            var rendererTrans = mainMonitor.transform;
            var pos = rendererTrans.position - rendererTrans.forward * cameraSettings.distance;
            cameraTrans.SetPositionAndRotation(pos, rendererTrans.rotation);
            camera.orthographic = true;
            camera.nearClipPlane = cameraSettings.near;
            camera.farClipPlane = cameraSettings.far;
            var bounds = mainMonitor.GetUnRotatedBounds();
            camera.orthographicSize = bounds.extents.y;
        }

        void SetupMonitorGridFibers()
        {
            if (monitorGridFibers == null)
            {
                monitorGridFibers = new List<MonitorGridFiber>();
            }

            monitorGridFibers.Clear();
            var part = (int) partitionType;
            var gridCount = Mathf.FloorToInt(part / 10f) * part % 10;
            for (var i = 0; i < gridCount; i++)
            {
                var gridRenderers = new List<LightSource>();
                gridRenderers.Add(mainMonitor.GetLightSource(i));
                gridRenderers.AddRange(subMonitors.Select(x => x.GetLightSource(i)));
                var traversedGrid = new MonitorGridFiber(gridRenderers.ToArray());
                monitorGridFibers.Add(traversedGrid);
            }
        }


        [Serializable]
        class CameraSettings
        {
            public float near = 0f;
            public float far = 0.2f;
            public float distance = 0.1f;
        }


        [Serializable]
        sealed class MonitorGridFiber : ILightSource
        {
            [SerializeField] LightSource[] lightSources;

            public MonitorGridFiber(LightSource[] lightSources)
            {
                this.lightSources = lightSources;
            }


            public void Initialize()
            {
                foreach (var lightSource in lightSources)
                {
                    lightSource.Initialize();
                }
            }

            public void TurnOff()
            {
                foreach (var lightSource in lightSources)
                {
                    lightSource.TurnOff();
                }
            }

            public void TurnOn()
            {
                foreach (var lightSource in lightSources)
                {
                    lightSource.TurnOn();
                }
            }

            public bool Includes(Object obj)
            {
                return lightSources.Any(l => l.Includes(obj));
            }

            public void Clear()
            {
                foreach (var lightSource in lightSources)
                {
                    lightSource.Clear();
                }
            }
        }
    }
}
