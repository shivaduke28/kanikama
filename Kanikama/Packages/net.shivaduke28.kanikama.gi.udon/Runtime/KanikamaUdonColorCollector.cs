using UdonSharp;
using UnityEngine;

namespace Kanikama.GI.Udon
{
    [RequireComponent(typeof(Camera)), UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KanikamaUdonColorCollector : UdonSharpBehaviour
    {
        [SerializeField] KanikamaUdonLightSource[] lightSources;
        [SerializeField] KanikamaUdonLightSourceGroup[] lightSourceGroups;

        [SerializeField] Vector4[] colors; // linear

        const int MaxColorCount = 64;

        public Vector4[] GetColors()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            return colors;
        }

        bool isInitialized;

        Color[][] lightSourceGroupColors;
        int[] lightSourceGroupStartIndex;

        void Initialize()
        {
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
        // KanikamaUdonGIUpdater (and other scripts) attached to the same GameObject should use them on or after OnPreRender.
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
        }
    }
}
