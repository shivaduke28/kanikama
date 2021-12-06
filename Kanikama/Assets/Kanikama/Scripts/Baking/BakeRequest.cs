using System.Collections.Generic;
using System.Linq;

namespace Kanikama.Baking
{
    public class BakeRequest
    {
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

        public BakeRequest(int sourceCount, int groupCount)
        {
            lightSourceFlags = Enumerable.Repeat(true, sourceCount).ToList();
            lightSourceGroupFlags = Enumerable.Repeat(true, groupCount).ToList();
        }
    }
}
