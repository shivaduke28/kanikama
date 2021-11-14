using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor
{
    public class BakeRequest
    {
        public KanikamaSceneDescriptor SceneDescriptor { get; }
        public bool isBakeAll = true;

        public List<bool> lightSourceFrags;
        public List<bool> lightSourceGroupFrags;

        public bool isBakeAmbient = true;
        public bool isGenerateAssets = true;
        public bool isBakeWithouKanikama = true;
        public bool isDirectionalMode = false;
        public bool createRenderTexture;
        public bool createCustomRenderTexture;

        public bool IsBakeLightSource(int index) => isBakeAll || lightSourceFrags[index];
        public bool IsBakeLightSourceGroup(int index) => isBakeAll || lightSourceGroupFrags[index];
        //public bool IsBakeAmbient() => isBakeAll || isBakeAmbient;
        public bool IsBakeWithouKanikama() => isBakeAll || isBakeWithouKanikama;

        public BakeRequest(KanikamaSceneDescriptor sceneDescriptor)
        {
            SceneDescriptor = sceneDescriptor;
            lightSourceFrags = Enumerable.Repeat(true, sceneDescriptor.GetLightSources().Count).ToList();
            lightSourceGroupFrags = Enumerable.Repeat(true, sceneDescriptor.GetLightSourceGroups().Count).ToList();
        }
    }
}
