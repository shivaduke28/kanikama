using Kanikama;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama
{
    [Serializable]
    public class KanikamaMonitorTraversedGrid : IKanikamaLightSource
    {
        [SerializeField] readonly List<Renderer> renderers;
        public KanikamaMonitorTraversedGrid(List<Renderer> renderers)
        {
            this.renderers = renderers;
        }

        #region IKanikamaLightSource
        public bool Contains(object obj)
        {
            return obj is Renderer r && renderers.Contains(r);
        }

        public void OnBake()
        {
            foreach (var renderer in renderers)
            {
                renderer.enabled = true;
            }
        }

        public void OnBakeSceneStart() { }

        public void Rollback()
        {
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }
        }

        public void TurnOff()
        {
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }
        }
        #endregion
    }
}