﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.GI.Runtime.Impl
{
    [AddComponentMenu("Kanikama/GI/Runtime.KanikamaGIUpdater")]
    public class KanikamaGIUpdater : MonoBehaviour
    {
        [SerializeField] KanikamaSceneDescriptor kanikamaSceneDescriptor;
        [SerializeField] Renderer[] renderers;
        [SerializeField] Texture2DArray[] lightmapArrays;
        [SerializeField] Texture2DArray[] directionalLightmapArrays;

        static readonly int LightmapArray = Shader.PropertyToID("_Udon_LightmapArray");
        static readonly int LightmapIndArray = Shader.PropertyToID("_Udon_LightmapIndArray");
        static readonly int Count = Shader.PropertyToID("_Udon_LightmapCount");
        static readonly int Colors = Shader.PropertyToID("_Udon_LightmapColors");

        Vector4[] colorsInternal;
        MaterialPropertyBlock block;
        List<ILightSource> lightSources;
        List<ILightSourceGroup> lightSourceGroups;
        List<IndexedColorArray> indexedColorArrays;

        void Start()
        {
            lightSources = kanikamaSceneDescriptor.GetLightSources();
            var index = lightSources.Count;
            indexedColorArrays = new List<IndexedColorArray>();

            foreach (var lightSourceGroup in kanikamaSceneDescriptor.GetLightSourceGroups)
            {
                var colors = lightSourceGroup.GetColors();
                var indexedArray = new IndexedColorArray(colors, index);
                indexedColorArrays.Add(indexedArray);
                index += indexedArray.Length;
            }
            colorsInternal = new Vector4[index];

            block = new MaterialPropertyBlock();
            foreach (var r in renderers)
            {
                var i = r.lightmapIndex;
                if (i < 0 || i >= lightmapArrays.Length)
                {
                    Debug.LogWarning($"invalid lightmap index. {r.name}: {i}");
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
        }

        // TODO: update colors on post render timing...
        void LateUpdate()
        {
            for (var i = 0; i < lightSources.Count; i++)
            {
                colorsInternal[i] = lightSources[i].GetColorLinear();
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
            Shader.SetGlobalInt(Count, colorsInternal.Length);
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
