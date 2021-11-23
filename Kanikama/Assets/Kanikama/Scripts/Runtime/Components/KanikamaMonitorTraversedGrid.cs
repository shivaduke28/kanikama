using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama
{
    [Serializable]
    public class KanikamaMonitorTraversedGrid : ILightSource
    {
        [SerializeField] List<KanikamaGridRenderer> gridRenderers;

        public KanikamaMonitorTraversedGrid(List<KanikamaGridRenderer> gridRenderers)
        {
            this.gridRenderers = gridRenderers;
        }

        public bool Contains(object obj)
        {
            return gridRenderers.Any(x => x.Contains(obj));
        }

        public void OnBake()
        {
            foreach (var source in gridRenderers)
            {
                source.OnBake();
            }
        }

        public void OnBakeSceneStart()
        {
            foreach (var source in gridRenderers)
            {
                source.OnBakeSceneStart();
            }
        }

        public void Rollback()
        {
            foreach (var source in gridRenderers)
            {
                source.Rollback();
            }
        }

        public void TurnOff()
        {
            foreach (var source in gridRenderers)
            {
                source.TurnOff();
            }
        }
    }
}
