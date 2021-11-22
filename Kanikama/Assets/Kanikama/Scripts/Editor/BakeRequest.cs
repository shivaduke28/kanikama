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

        public List<bool> lightSourceFlags;
        public List<bool> lightSourceGroupFlags;

        public bool isBakeAmbient = true;
        public bool isGenerateAssets = true;
        public bool isBakeWithouKanikama = true;
        public bool isDirectionalMode = false;
        public bool createRenderTexture;
        public bool createCustomRenderTexture;

        public bool IsBakeLightSource(int index) => isBakeAll || lightSourceFlags[index];
        public bool IsBakeLightSourceGroup(int index) => isBakeAll || lightSourceGroupFlags[index];
        public bool IsBakeWithouKanikama() => isBakeAll || isBakeWithouKanikama;

        public BakeRequest(KanikamaSceneDescriptor sceneDescriptor)
        {
            SceneDescriptor = sceneDescriptor;
            lightSourceFlags = Enumerable.Repeat(true, sceneDescriptor.GetLightSources().Count).ToList();
            lightSourceGroupFlags = Enumerable.Repeat(true, sceneDescriptor.GetLightSourceGroups().Count).ToList();
        }
    }
}
