﻿using System;
using System.Collections.Generic;
using System.Linq;
using Kanikama.Attributes;
using UnityEngine;

namespace Kanikama
{
    public sealed class KanikamaMonitorGroupHolder : MonoBehaviour
    {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        [SerializeField, NonNull] KanikamaMonitorV2 mainMonitor;

        [SerializeField, NonNull] KanikamaMonitorV2[] subMonitors;
        [SerializeField, NonNull] KanikamaLightSource gridCellPrefab;
        [SerializeField] List<MonitorGridFiber> monitorGridFibers;

        [Serializable]
        sealed class MonitorGridFiber : IKanikamaBakeTarget
        {
            [SerializeField] KanikamaLightSource[] lightSources;

            public MonitorGridFiber(KanikamaLightSource[] lightSources)
            {
                this.lightSources = lightSources;
            }

            public void Initialize()
            {
                foreach (var bakeTarget in lightSources)
                {
                    bakeTarget.Initialize();
                    bakeTarget.gameObject.SetActive(true);
                }
            }

            public void TurnOff()
            {
                foreach (var bakeTarget in lightSources)
                {
                    bakeTarget.TurnOff();
                }
            }

            public void TurnOn()
            {
                foreach (var bakeTarget in lightSources)
                {
                    bakeTarget.TurnOn();
                }
            }

            public void Clear()
            {
                foreach (var bakeTarget in lightSources)
                {
                    bakeTarget.gameObject.SetActive(false);
                    bakeTarget.Clear();
                }
            }
        }

        public List<IKanikamaBakeTarget> GetAll() => monitorGridFibers.Cast<IKanikamaBakeTarget>().ToList();

        public IKanikamaBakeTarget Get(int index) => monitorGridFibers[index];


        public void Setup(KanikamaMonitorPartitionType partitionType)
        {
            // baking
            if (mainMonitor == null) return;
            mainMonitor.SetupLights(partitionType, gridCellPrefab);
            foreach (var monitor in subMonitors)
            {
                monitor.SetupLights(partitionType, gridCellPrefab);
            }
            SetupMonitorGridFibers(partitionType);
        }

        void SetupMonitorGridFibers(KanikamaMonitorPartitionType partitionType)
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
                var gridRenderers = new List<KanikamaLightSource>();
                gridRenderers.Add(mainMonitor.GetLightSource(i));
                gridRenderers.AddRange(subMonitors.Select(x => x.GetLightSource(i)));
                var traversedGrid = new MonitorGridFiber(gridRenderers.ToArray());
                monitorGridFibers.Add(traversedGrid);
            }
        }
#endif
    }
}
