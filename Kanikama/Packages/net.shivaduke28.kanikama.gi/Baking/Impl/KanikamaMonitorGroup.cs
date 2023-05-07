using System;
using System.Collections.Generic;
using System.Linq;
using Kanikama.Core.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.GI.Baking.Impl
{
    [AddComponentMenu("Kanikama/Baking.KanikamaMonitorGroup")]
    [EditorOnly]
    public sealed class KanikamaMonitorGroup : BakeTargetGroup
    {
        [SerializeField] KanikamaMonitor mainMonitor;
        [SerializeField] KanikamaMonitor[] subMonitors;
        [SerializeField] BakeTarget bakeTargetPrefab;
        [SerializeField] KanikamaMonitor.PartitionType partitionType;
        [SerializeField] List<MonitorGridFiber> monitorGridFibers;


        [Serializable]
        sealed class MonitorGridFiber : IBakeTarget
        {
            [SerializeField] BakeTarget[] bakeTargets;

            public MonitorGridFiber(BakeTarget[] bakeTargets)
            {
                this.bakeTargets = bakeTargets;
            }


            public void Initialize()
            {
                foreach (var lightSource in bakeTargets)
                {
                    lightSource.Initialize();
                }
            }

            public void TurnOff()
            {
                foreach (var lightSource in bakeTargets)
                {
                    lightSource.TurnOff();
                }
            }

            public void TurnOn()
            {
                foreach (var lightSource in bakeTargets)
                {
                    lightSource.TurnOn();
                }
            }

            public bool Includes(Object obj)
            {
                return bakeTargets.Any(l => l.Includes(obj));
            }

            public void Clear()
            {
                foreach (var lightSource in bakeTargets)
                {
                    lightSource.Clear();
                }
            }
        }

        public override List<IBakeTarget> GetAll() => monitorGridFibers.Cast<IBakeTarget>().ToList();
        public override IBakeTarget Get(int index) => monitorGridFibers[index];

        public void Setup()
        {
            if (mainMonitor == null) return;
            mainMonitor.SetupLights(partitionType, bakeTargetPrefab);
            foreach (var monitor in subMonitors)
            {
                monitor.SetupLights(partitionType, bakeTargetPrefab);
            }
            SetupMonitorGridFibers();
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
                var gridRenderers = new List<BakeTarget>();
                gridRenderers.Add(mainMonitor.GetBakeTarget(i));
                gridRenderers.AddRange(subMonitors.Select(x => x.GetBakeTarget(i)));
                var traversedGrid = new MonitorGridFiber(gridRenderers.ToArray());
                monitorGridFibers.Add(traversedGrid);
            }
        }
    }
}
