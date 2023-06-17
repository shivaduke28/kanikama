using System.Collections.Generic;
using Kanikama.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kanikama.Application.Impl
{
    public sealed class KanikamaRuntimeGIUpdater : MonoBehaviour
    {
        [SerializeField] bool isSRP;
        [SerializeField] Camera targetCamera;
        [SerializeField] List<LightSource> lightSources;
        [SerializeField] List<LightSourceGroup> lightSourceGroups;
        [SerializeField] Renderer[] renderers;
        [SerializeField] Texture2DArray[] lightmapArrays;
        [SerializeField] Texture2DArray[] directionalLightmapArrays;

        static readonly int LightmapArray = Shader.PropertyToID("_Udon_LightmapArray");
        static readonly int LightmapIndArray = Shader.PropertyToID("_Udon_LightmapIndArray");
        static readonly int Count = Shader.PropertyToID("_Udon_LightmapCount");
        static readonly int Colors = Shader.PropertyToID("_Udon_LightmapColors");
        const int MaxColorCount = 64;

        Vector4[] colorsInternal;
        MaterialPropertyBlock block;
        List<IndexedColorArray> indexedColorArrays;

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
            foreach (var r in renderers)
            {
                var i = r.lightmapIndex;
                if (i < 0 || i >= lightmapArrays.Length)
                {
                    Debug.LogWarningFormat(KanikamaDebug.Format, $"invalid lightmap index. {r.name}: {i}");
                    continue;
                }

                r.GetPropertyBlock(block);
                block.SetTexture(LightmapArray, lightmapArrays[i]);
                if (i < directionalLightmapArrays.Length)
                {
                    block.SetTexture(LightmapIndArray, directionalLightmapArrays[i]);
                }
                r.SetPropertyBlock(block);
            }
            Shader.SetGlobalInt(Count, index);
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
    }
}
