using System.Collections.Generic;
using System.Linq;

namespace Kanikama.Baking
{
    public class BakeRequest
    {
        public bool IsBakeAll = true;

        public readonly List<bool> LightSourceFlags;
        public readonly List<bool> LightSourceGroupFlags;

        public bool IsGenerateAssets = true;
        public bool IsBakeNonKanikama = true;
        public bool IsDirectionalMode = false;
        public bool IsPackTextures = false;
        public bool IsCreateRenderTexture;
        public bool IsCreateCustomRenderTexture;

        public bool IsBakeLightSource(int index) => IsBakeAll || LightSourceFlags[index];
        public bool IsBakeLightSourceGroup(int index) => IsBakeAll || LightSourceGroupFlags[index];
        public bool IsBakeWithoutKanikama() => IsBakeAll || IsBakeNonKanikama;

        public BakeRequest(int sourceCount, int groupCount)
        {
            LightSourceFlags = Enumerable.Repeat(true, sourceCount).ToList();
            LightSourceGroupFlags = Enumerable.Repeat(true, groupCount).ToList();
        }
    }
}
