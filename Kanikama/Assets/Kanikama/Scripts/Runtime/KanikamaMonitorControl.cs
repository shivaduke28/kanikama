using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Kanikama
{
    [RequireComponent(typeof(Camera))]
    public class KanikamaMonitorControl : KanikamaLightSourceGroup
    {
        [SerializeField] Camera camera;
        [SerializeField] Renderer gridRendererPrefab;
        [SerializeField] KanikamaMonitor.PartitionType partitionType;
        [SerializeField] KanikamaMonitorQuad mainMonitor;

        [SerializeField] List<KanikamaMonitor> subMonitors;
        [SerializeField] CameraDetailedSettings cameraDetailedSettings;
        [SerializeField, HideInInspector] List<KanikamaMonitorTraversedGrid> traversedGrids;

        public Camera Camera => camera;
        public KanikamaMonitor.PartitionType PartitionType => partitionType;
        public KanikamaMonitorQuad MainMonitor => mainMonitor;

        void OnValidate()
        {
            if (camera == null) camera = GetComponent<Camera>();
        }

        public void Setup()
        {
            if (mainMonitor == null) return;
            mainMonitor.SetupLights(partitionType, gridRendererPrefab);
            foreach (var monitor in subMonitors)
            {
                monitor.SetupLights(partitionType, gridRendererPrefab);
            }
            SetupCamera();
        }

        void SetupCamera()
        {
            var cameraTrans = camera.transform;
            var rendererTrans = mainMonitor.transform;
            var pos = rendererTrans.position - rendererTrans.forward * cameraDetailedSettings.distance;
            cameraTrans.SetPositionAndRotation(pos, rendererTrans.rotation);
            camera.nearClipPlane = cameraDetailedSettings.near;
            camera.farClipPlane = cameraDetailedSettings.far;
            var bounds = mainMonitor.GetUnrotatedBounds();
            camera.orthographicSize = bounds.extents.y;
        }

        [Serializable]
        class CameraDetailedSettings
        {
            public float near = 0f;
            public float far = 0.2f;
            public float distance = 0.1f;
        }

        #region KanikamaLightSourceGroup
        public override bool Contains(object obj)
        {
            if (obj is Renderer r)
            {
                return mainMonitor.monitorRenderer == r || subMonitors.Any(x => x.monitorRenderer == r);
            }
            return false;
        }

        public override IReadOnlyList<IKanikamaLightSource> GetLightSources() => traversedGrids.AsReadOnly();

        public override void Rollback()
        {
            mainMonitor.monitorRenderer.enabled = true;
            foreach (var m in subMonitors)
            {
                m.monitorRenderer.enabled = true;
            }
            traversedGrids = null;
        }

        public override void OnBakeSceneStart()
        {
            mainMonitor.monitorRenderer.enabled = false;
            foreach (var m in subMonitors)
            {
                m.monitorRenderer.enabled = false;
            }

            InitializeTraversedGrids();
        }
        #endregion

        void InitializeTraversedGrids()
        {
            var monitorCount = 1 + subMonitors.Count;
            var part = (int)partitionType;
            var gridCount = Mathf.FloorToInt(part / 10f) + part % 10;
            traversedGrids = new List<KanikamaMonitorTraversedGrid>();
            for (var i = 0; i < gridCount; i++)
            {
                var gridRenderers = new List<Renderer>();
                gridRenderers.Add(mainMonitor.gridRenderers[i]);
                gridRenderers.AddRange(subMonitors.Select(x => x.gridRenderers[i]));
                var traversedGrid = new KanikamaMonitorTraversedGrid(gridRenderers);
                traversedGrids.Add(traversedGrid);
            }
        }
    }
}