using System;
using System.Collections.Generic;
using System.Linq;
using Kanikama.Baking.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.Baking.Impl
{
    public sealed class KanikamaBakeTargetMonitorGroup : BakeTargetGroup
    {
        [SerializeField, NonNull] KanikamaMonitor mainMonitor;
        [SerializeField, NonNull] KanikamaMonitor[] subMonitors;
        [SerializeField, NonNull] BakeTarget bakeTargetPrefab;
        [SerializeField] KanikamaMonitor.PartitionType partitionType = KanikamaMonitor.PartitionType.Grid1x1;
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
                foreach (var bakeTarget in bakeTargets)
                {
                    bakeTarget.Initialize();
                    bakeTarget.gameObject.SetActive(true);
                }
            }

            public void TurnOff()
            {
                foreach (var bakeTarget in bakeTargets)
                {
                    bakeTarget.TurnOff();
                }
            }

            public void TurnOn()
            {
                foreach (var bakeTarget in bakeTargets)
                {
                    bakeTarget.TurnOn();
                }
            }

            public bool Includes(Object obj)
            {
                return bakeTargets.Any(l => l.Includes(obj));
            }

            public void Clear()
            {
                foreach (var bakeTarget in bakeTargets)
                {
                    bakeTarget.gameObject.SetActive(false);
                    bakeTarget.Clear();
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
