using System.Linq;
using Kanikama.Attributes;
using UnityEngine;

namespace Kanikama
{
    public class KanikamaManager : KanikamaBehaviour
    {
        [SerializeField, NonNull] KanikamaLightSource[] lightSources;

        [SerializeField, NonNull] KanikamaLightSourceGroup[] lightSourceGroups;
        [SerializeField, NonNull] Renderer[] receivers;

        [SerializeField, NonNull] Texture[] lightmapArrays;
        [SerializeField, NonNull] Texture[] directionalLightmapArrays;
        [SerializeField] int sliceCount;
        [SerializeField] Vector4[] colors; // linear


        const int MaxColorCount = 64;
        bool isInitialized;

        Color[][] lightSourceGroupColors;
        int[] lightSourceGroupStartIndex;

        MaterialPropertyBlock block;

        // shader properties
        int lightmapArrayId;
        int lightmapIndArrayId;
        int countId;
        int colorsId;

        [Header("LTC")] [SerializeField] bool enableLtc;
        [SerializeField, NonNull] Transform[] ltcMonitors;

        [SerializeField, NonNull] Texture[] ltcVisibilityMaps;
        [SerializeField, NonNull] Texture ltcLut0;
        [SerializeField, NonNull] Texture ltcLut1;
        [SerializeField, NonNull] Texture ltcLightSourceTex;

        int ltcCount;
        Vector4[] vertex0;
        Vector4[] vertex1;
        Vector4[] vertex2;
        Vector4[] vertex3;

        // shader properties
        int ltcCountId;
        int ltcVertex0Id;
        int ltcVertex1Id;
        int ltcVertex2Id;
        int ltcVertex3Id;
        int ltcTex0Id;
        int ltcTex1Id;
        int ltcLightTex0Id;
        int visibilityMapId;


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

            lightmapArrayId = KanikamaShader.PropertyToID("_Udon_LightmapArray");
            lightmapIndArrayId = KanikamaShader.PropertyToID("_Udon_LightmapIndArray");
            countId = KanikamaShader.PropertyToID("_Udon_LightmapCount");
            colorsId = KanikamaShader.PropertyToID("_Udon_LightmapColors");

            // LTC
            ltcCountId = KanikamaShader.PropertyToID("_Udon_LTC_Count");
            visibilityMapId = KanikamaShader.PropertyToID("_Udon_LTC_VisibilityMap");
            ltcVertex0Id = KanikamaShader.PropertyToID("_Udon_LTC_Vertex0");
            ltcVertex1Id = KanikamaShader.PropertyToID("_Udon_LTC_Vertex1");
            ltcVertex2Id = KanikamaShader.PropertyToID("_Udon_LTC_Vertex2");
            ltcVertex3Id = KanikamaShader.PropertyToID("_Udon_LTC_Vertex3");
            ltcTex0Id = KanikamaShader.PropertyToID("_Udon_LTC_LUT0");
            ltcTex1Id = KanikamaShader.PropertyToID("_Udon_LTC_LUT1");
            ltcLightTex0Id = KanikamaShader.PropertyToID("_Udon_LTC_LightTex0");

            block = new MaterialPropertyBlock();
            var directionalMapCount = directionalLightmapArrays == null ? -1 : directionalLightmapArrays.Length - 1;
            foreach (var r in receivers)
            {
                var lmi = r.lightmapIndex;
                if (lmi < 0 || lmi >= lightmapArrays.Length) continue;
                r.GetPropertyBlock(block);
                block.SetTexture(lightmapArrayId, lightmapArrays[lmi]);
                if (lmi <= directionalMapCount)
                {
                    block.SetTexture(lightmapIndArrayId, directionalLightmapArrays[lmi]);
                }

                if (enableLtc && lmi < ltcVisibilityMaps.Length)
                {
                    block.SetTexture(visibilityMapId, ltcVisibilityMaps[lmi]);
                }

                block.SetInt(countId, sliceCount);
                r.SetPropertyBlock(block);
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

            if (enableLtc)
            {
                vertex0 = new Vector4[3];
                vertex1 = new Vector4[3];
                vertex2 = new Vector4[3];
                vertex3 = new Vector4[3];

                ltcCount = Mathf.Min(3, ltcMonitors.Length);
                KanikamaShader.SetGlobalInteger(ltcCountId, ltcCount);
                KanikamaShader.SetGlobalTexture(ltcTex0Id, ltcLut0);
                KanikamaShader.SetGlobalTexture(ltcTex1Id, ltcLut1);
                KanikamaShader.SetGlobalTexture(ltcLightTex0Id, ltcLightSourceTex);

                UpdateLtcVertexPositions();
            }

            isInitialized = true;
        }

        void UpdateLtcVertexPositions()
        {
            // Unity's Quad mesh
            var w = 0.5f;
            var h = 0.5f;

            for (var i = 0; i < ltcCount; i++)
            {
                var localToWorld = ltcMonitors[i].localToWorldMatrix;

                var p0 = new Vector3(w, -h, 0);
                var p1 = new Vector3(-w, -h, 0);
                var p2 = new Vector3(-w, h, 0);
                var p3 = new Vector3(w, h, 0);

                p0 = localToWorld.MultiplyPoint3x4(p0);
                p1 = localToWorld.MultiplyPoint3x4(p1);
                p2 = localToWorld.MultiplyPoint3x4(p2);
                p3 = localToWorld.MultiplyPoint3x4(p3);

                vertex0[i] = p0;
                vertex1[i] = p1;
                vertex2[i] = p2;
                vertex3[i] = p3;
            }

            KanikamaShader.SetGlobalVectorArray(ltcVertex0Id, vertex0);
            KanikamaShader.SetGlobalVectorArray(ltcVertex1Id, vertex1);
            KanikamaShader.SetGlobalVectorArray(ltcVertex2Id, vertex2);
            KanikamaShader.SetGlobalVectorArray(ltcVertex3Id, vertex3);
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
            KanikamaShader.SetGlobalVectorArray(colorsId, colors);
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public KanikamaLightSource[] GetBakeTargets() => lightSources.ToArray();
        public KanikamaLightSourceGroup[] GetBakeTargetGroups() => lightSourceGroups.ToArray();
#endif
    }
}
