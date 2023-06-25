using System.Collections.Generic;
using System.Linq;
using Kanikama.Baking.Attributes;
using Kanikama.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kanikama.Application.Impl
{
    public sealed class KanikamaRuntimeGIUpdater : MonoBehaviour
    {
        [SerializeField] bool isSRP;
        [SerializeField] Camera targetCamera;

        [Header("Scene")]
        [SerializeField, NonNull]
        List<LightSource> lightSources;

        [SerializeField, NonNull] List<LightSourceGroup> lightSourceGroups;
        [SerializeField, NonNull] Renderer[] receivers;
        [SerializeField, NonNull] Texture2DArray[] lightmapArrays;
        [SerializeField, NonNull] Texture2DArray[] directionalLightmapArrays;

        static readonly int LightmapArray = Shader.PropertyToID("_Udon_LightmapArray");
        static readonly int LightmapIndArray = Shader.PropertyToID("_Udon_LightmapIndArray");
        static readonly int Count = Shader.PropertyToID("_Udon_LightmapCount");
        static readonly int Colors = Shader.PropertyToID("_Udon_LightmapColors");
        const int MaxColorCount = 64;

        Vector4[] colorsInternal;
        MaterialPropertyBlock block;
        List<IndexedColorArray> indexedColorArrays;

        [Header("LTC")] [SerializeField] bool enableLtc;
        [SerializeField] Transform[] ltcMonitors;
        [SerializeField, NonNull] Texture[] ltcVisibilityMaps;
        [SerializeField, NonNull] Texture ltcLut0;
        [SerializeField, NonNull] Texture ltcLut1;
        [SerializeField, NonNull] Texture ltcLightSourceTex;

        readonly Vector4[] vertex0 = new Vector4[3];
        readonly Vector4[] vertex1 = new Vector4[3];
        readonly Vector4[] vertex2 = new Vector4[3];
        readonly Vector4[] vertex3 = new Vector4[3];

        static readonly int LtcCount = Shader.PropertyToID("_Udon_LTC_Count");
        static readonly int LtcVisibilityMap = Shader.PropertyToID("_Udon_LTC_VisibilityMap");
        static readonly int LtcVertex0 = Shader.PropertyToID("_Udon_LTC_Vertex0");
        static readonly int LtcVertex1 = Shader.PropertyToID("_Udon_LTC_Vertex1");
        static readonly int LtcVertex2 = Shader.PropertyToID("_Udon_LTC_Vertex2");
        static readonly int LtcVertex3 = Shader.PropertyToID("_Udon_LTC_Vertex3");
        static readonly int UdonLtcLut0 = Shader.PropertyToID("_Udon_LTC_LUT0");
        static readonly int UdonLtcLut1 = Shader.PropertyToID("_Udon_LTC_LUT1");
        static readonly int UdonLtcLightTex0 = Shader.PropertyToID("_Udon_LTC_LightTex0");

        int ltcMonitorCount;

        void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == targetCamera)
            {
                UpdateColors();
            }
        }

        void OnPreRenderCallback(Camera camera)
        {
            if (camera == targetCamera)
            {
                UpdateColors();
            }
        }

        void OnEnable()
        {
            if (isSRP)
            {
                RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            }
            else
            {
                Camera.onPreRender += OnPreRenderCallback;
            }
        }

        void OnDisable()
        {
            if (isSRP)
            {
                RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            }
            else
            {
                Camera.onPreRender -= OnPreRenderCallback;
            }
        }

        void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    enabled = false;
                    return;
                }
            }

            var index = lightSources.Count;
            indexedColorArrays = new List<IndexedColorArray>();

            foreach (var lightSourceGroup in lightSourceGroups)
            {
                var colors = lightSourceGroup.GetLinearColors();
                var indexedArray = new IndexedColorArray(colors, index);
                indexedColorArrays.Add(indexedArray);
                index += indexedArray.Length;
            }
            colorsInternal = new Vector4[MaxColorCount];

            block = new MaterialPropertyBlock();
            foreach (var r in receivers)
            {
                var i = r.lightmapIndex;
                if (i < 0 || i >= lightmapArrays.Length)
                {
                    Debug.LogWarningFormat(KanikamaDebug.Format, $"invalid lightmap index. {r.name}: {i}");
                    continue;
                }

                r.GetPropertyBlock(block);

                // lightmap
                block.SetTexture(LightmapArray, lightmapArrays[i]);
                // directional map
                if (i < directionalLightmapArrays.Length)
                {
                    block.SetTexture(LightmapIndArray, directionalLightmapArrays[i]);
                }
                // ltc visibility
                if (enableLtc && i < ltcVisibilityMaps.Length)
                {
                    block.SetTexture(LtcVisibilityMap, ltcVisibilityMaps[i]);
                }

                r.SetPropertyBlock(block);
            }

            Shader.SetGlobalInt(Count, index);

            if (enableLtc)
            {
                ltcMonitorCount = Mathf.Min(3, ltcMonitors.Length);
                Shader.SetGlobalInt(LtcCount, ltcMonitorCount);
                Shader.SetGlobalTexture(UdonLtcLut0, ltcLut0);
                Shader.SetGlobalTexture(UdonLtcLut1, ltcLut1);
                Shader.SetGlobalTexture(UdonLtcLightTex0, ltcLightSourceTex);

                UpdateLtcVertexPositions();
            }
        }

        void UpdateColors()
        {
            for (var i = 0; i < lightSources.Count; i++)
            {
                colorsInternal[i] = lightSources[i].GetLinearColor();
            }

            foreach (var indexedColorArray in indexedColorArrays)
            {
                var start = indexedColorArray.StartIndex;
                var length = indexedColorArray.Length;
                var colors = indexedColorArray.Colors;
                for (var i = 0; i < length; i++)
                {
                    colorsInternal[start + i] = colors[i];
                }
            }

            Shader.SetGlobalVectorArray(Colors, colorsInternal);
        }

        void UpdateLtcVertexPositions()
        {
            // Unity's Quad mesh
            var w = 0.5f;
            var h = 0.5f;

            for (var i = 0; i < ltcMonitorCount; i++)
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

            Shader.SetGlobalVectorArray(LtcVertex0, vertex0);
            Shader.SetGlobalVectorArray(LtcVertex1, vertex1);
            Shader.SetGlobalVectorArray(LtcVertex2, vertex2);
            Shader.SetGlobalVectorArray(LtcVertex3, vertex3);
        }

        class IndexedColorArray
        {
            public Color[] Colors { get; }
            public int Length { get; }
            public int StartIndex { get; }

            public IndexedColorArray(Color[] colors, int startIndex)
            {
                Colors = colors;
                Length = colors.Length;
                StartIndex = startIndex;
            }
        }

        public bool Validate()
        {
            return receivers.All(x => x != null)
                && lightmapArrays.All(x => x != null)
                && directionalLightmapArrays.All(x => x != null)
                && lightSources.All(x => x != null)
                && lightSourceGroups.All(x => x != null)
                && !enableLtc || (ltcMonitors.All(x => x != null)
                    && ltcVisibilityMaps.All(x => x != null)
                    && ltcLut0 != null
                    && ltcLut1 != null
                    && ltcLightSourceTex != null);
        }
    }
}
