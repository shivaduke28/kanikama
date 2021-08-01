using Kanikama.EditorOnly;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kanikama.Editor
{
    public class BakeRequest
    {
        public KanikamaSceneDescriptor SceneDescriptor { get; }
        public bool isBakeAll = true;

        public List<bool> lightRequests;
        public List<bool> rendererRequests;
        public List<bool> monitorRequests;

        public bool isBakeAmbient = true;
        public bool isGenerateAssets = true;
        public bool isBakeWithouKanikama = true;

        public bool IsLightRequested(int index) => isBakeAll || lightRequests[index];
        public bool IsRendererRequested(int index) => isBakeAll || rendererRequests[index];
        public bool IsMonitorRequested(int index) => isBakeAll || monitorRequests[index];
        public bool IsBakeAmbient() => isBakeAll || isBakeAmbient;
        public bool IsBakeWithouKanikama() => isBakeAll || isBakeWithouKanikama;


        public BakeRequest(KanikamaSceneDescriptor sceneDescriptor)
        {
            SceneDescriptor = sceneDescriptor;
            lightRequests = Enumerable.Repeat(true, sceneDescriptor.Lights.Count).ToList();
            rendererRequests = Enumerable.Repeat(true, sceneDescriptor.EmissiveRenderers.Count).ToList();
            monitorRequests = Enumerable.Repeat(true, sceneDescriptor.MonitorSetups.Count).ToList();
            isBakeAmbient = sceneDescriptor.IsAmbientEnable;
        }
    }
}
