using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kanikama.Udon
{
    [RequireComponent(typeof(Camera)), UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KanikamaUdonGIUpdater : UdonSharpBehaviour
    {
        [SerializeField] Renderer[] receivers;

        [SerializeField] Texture[] lightmapArrays;
        [SerializeField] Texture[] directionalLightmapArrays;
        [SerializeField] int sliceCount;

        [SerializeField] KanikamaUdonLightSource[] lightSources;
        [SerializeField] KanikamaUdonLightSourceGroup[] lightSourceGroups;
        [SerializeField] Vector4[] colors; // linear

        const int MaxColorCount = 64;
        bool isInitialized;

        Color[][] lightSourceGroupColors;
        int[] lightSourceGroupStartIndex;

        MaterialPropertyBlock block;

        int lightmapArrayId;
        int lightmapIndArrayId;
        int countId;
        int colorsId;

        void Start()
        {
            Initialize();
        }

        public Vector4[] GetColors()
        {
            Initialize();
            return colors;
        }


        void Initialize()
        {
            if (isInitialized) return;

            lightmapArrayId = VRCShader.PropertyToID("_Udon_LightmapArray");
            lightmapIndArrayId = VRCShader.PropertyToID("_Udon_LightmapIndArray");
            countId = VRCShader.PropertyToID("_Udon_LightmapCount");
            colorsId = VRCShader.PropertyToID("_Udon_LightmapColors");

            block = new MaterialPropertyBlock();
            var directionalMapCount = directionalLightmapArrays == null ? -1 : directionalLightmapArrays.Length - 1;
            foreach (var renderer in receivers)
            {
                var lmi = renderer.lightmapIndex;
                if (lmi < 0) continue;
                renderer.GetPropertyBlock(block);
                block.SetTexture(lightmapArrayId, lightmapArrays[lmi]);
                if (lmi <= directionalMapCount)
                {
                    block.SetTexture(lightmapIndArrayId, directionalLightmapArrays[lmi]);
                }
                block.SetInt(countId, sliceCount);
                renderer.SetPropertyBlock(block);
            }

            colors = new Vector4[MaxColorCount];
            var index = lightSources.Length;

            var lightSourceGroupCount = lightSourceGroups.Length;
            lightSourceGroupColors = new Color[lightSourceGroupCount][];
            lightSourceGroupStartIndex = new int[lightSourceGroupCount];
            for (var i = 0; i < lightSourceGroupCount; i++)
            {
                var lightSourceGroup = lightSourceGroups[i];
                var groupColors = lightSourceGroup.GetLinearColors();
                lightSourceGroupColors[i] = groupColors;
                lightSourceGroupStartIndex[i] = index;
                index += groupColors.Length;
            }

            isInitialized = true;
        }

        // Note:
        // Colors are updated on OnPreCull in every frame,
        // Use colors on or after OnPreRender.
        void OnPreCull()
        {
            for (var i = 0; i < lightSources.Length; i++)
            {
                colors[i] = lightSources[i].GetLinearColor();
            }

            for (var i = 0; i < lightSourceGroups.Length; i++)
            {
                var groupColors = lightSourceGroupColors[i];
                var offset = lightSourceGroupStartIndex[i];
                for (var j = 0; j < groupColors.Length; j++)
                {
                    colors[offset + j] = groupColors[j];
                }
            }
            VRCShader.SetGlobalVectorArray(colorsId, colors);
        }
    }
}
