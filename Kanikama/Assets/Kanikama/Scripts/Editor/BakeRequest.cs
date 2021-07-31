using Amazon.S3.Model;
using Kanikama.EditorOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.Editor
{
    [Serializable]
    public class BakeRequest
    {
        public KanikamaSceneDescriptor SceneDescriptor { get; }
        public bool bakeAll = true;

        public List<bool> lightRequests;
        public List<bool> rendererRequests;
        public List<bool> monitorRequests;

        public bool bakeAmbient;
        public bool generateAssets;
        public bool bakeWithoutKanikama;

        public bool IsLightRequested(int index) => bakeAll || lightRequests[index];
        public bool IsRendererRequested(int index) => bakeAll || rendererRequests[index];
        public bool IsMonitorRequested(int index) => bakeAll || monitorRequests[index];
        public bool IsBakeAmbient() => bakeAll || bakeWithoutKanikama;
        public bool IsBakeWithouKanikama() => bakeAll || bakeWithoutKanikama;


        public BakeRequest(KanikamaSceneDescriptor sceneDescriptor)
        {
            SceneDescriptor = sceneDescriptor;
            lightRequests = Enumerable.Repeat(true, sceneDescriptor.Lights.Count).ToList();
            rendererRequests = Enumerable.Repeat(true, sceneDescriptor.EmissiveRenderers.Count).ToList();
            monitorRequests = Enumerable.Repeat(true, sceneDescriptor.MonitorSetups.Count).ToList();
        }

    }
}
