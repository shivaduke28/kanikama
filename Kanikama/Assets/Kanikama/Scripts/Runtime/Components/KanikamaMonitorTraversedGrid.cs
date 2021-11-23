using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Kanikama
{
    public class KanikamaMonitorTraversedGrid : LightSource
    {
        [SerializeField] public List<KanikamaGridRenderer> gridRenderers;
        public override bool Contains(object obj)
        {
            return gridRenderers.Any(x => x.Contains(obj));
        }

        public override void OnBake()
        {
            foreach (var source in gridRenderers)
            {
                source.OnBake();
            }
        }

        public override void OnBakeSceneStart()
        {
            foreach (var source in gridRenderers)
            {
                source.OnBakeSceneStart();
            }
        }

        public override void Rollback()
        {
            foreach (var source in gridRenderers)
            {
                source.Rollback();
            }
        }

        public override void TurnOff()
        {
            foreach (var source in gridRenderers)
            {
                source.TurnOff();
            }
        }
    }
}
