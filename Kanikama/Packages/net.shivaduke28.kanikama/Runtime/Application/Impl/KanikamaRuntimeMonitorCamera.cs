﻿using System;
using Kanikama.Baking.Impl;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kanikama.Application.Impl
{
    [RequireComponent(typeof(Camera))]
    public sealed class KanikamaRuntimeMonitorCamera : LightSourceGroup
    {
        [SerializeField] bool isSRP;
        [SerializeField] Renderer monitorRenderer;
        [SerializeField] KanikamaBakeTargetMonitor.PartitionType partitionType = KanikamaBakeTargetMonitor.PartitionType.Grid1x1;
        [SerializeField] Camera targetCamera;
        [SerializeField] CameraSettings cameraSettings;
        [SerializeField] Texture2D readingTexture;
        [SerializeField] Color[] colors;
        [SerializeField] float aspectRatio = 1f;

        [Serializable]
        class CameraSettings
        {
            public float near = 0f;
            public float far = 0.2f;
            public float distance = 0.1f;
        }

        Color[] colorsBuffer;

        int lightCount;
        int bufferCount;
        bool isUniform;
        bool isInitialized;

        int mipmapStartIndex;

        const int Mipmap6StartIndex = 256 * 256 + 128 * 128 + 64 * 64 + 32 * 32 + 16 * 16 + 8 * 8;
        const int Mipmap7StartIndex = 256 * 256 + 128 * 128 + 64 * 64 + 32 * 32 + 16 * 16 + 8 * 8 + 4 * 4;
        const int Mipmap8StartIndex = 256 * 256 + 128 * 128 + 64 * 64 + 32 * 32 + 16 * 16 + 8 * 8 + 4 * 4 + 2 * 2;

        public override Color[] GetLinearColors()
        {
            if (!isInitialized) Initialize();
            return colors;
        }

        void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == targetCamera)
            {
                UpdateColors();
            }
        }

        void OnPostRenderCallback(Camera camera)
        {
            if (camera == targetCamera)
            {
                UpdateColors();
            }
        }

        void OnValidate()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }
        }

        void Awake()
        {
            if (!isInitialized) Initialize();
        }

        void OnEnable()
        {
            if (isSRP)
            {
                RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            }
            else
            {
                Camera.onPostRender += OnPostRenderCallback;
            }
        }

        void OnDisable()
        {
            if (isSRP)
            {
                RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            }
            else
            {
                Camera.onPostRender -= OnPostRenderCallback;
            }
        }

        void Initialize()
        {
            targetCamera.aspect = aspectRatio;
            switch (partitionType)
            {
                case KanikamaBakeTargetMonitor.PartitionType.Grid1x1:
                    isUniform = true;
                    lightCount = 1;
                    bufferCount = lightCount;
                    mipmapStartIndex = Mipmap8StartIndex;
                    break;
                case KanikamaBakeTargetMonitor.PartitionType.Grid2x2:
                    isUniform = true;
                    lightCount = 4;
                    bufferCount = lightCount;
                    mipmapStartIndex = Mipmap7StartIndex;
                    break;
                case KanikamaBakeTargetMonitor.PartitionType.Grid4x4:
                    isUniform = true;
                    lightCount = 16;
                    bufferCount = lightCount;
                    mipmapStartIndex = Mipmap6StartIndex;
                    break;
                case KanikamaBakeTargetMonitor.PartitionType.Grid3x2:
                    lightCount = 6;
                    bufferCount = 16;
                    isUniform = false;
                    mipmapStartIndex = Mipmap6StartIndex;
                    break;
                case KanikamaBakeTargetMonitor.PartitionType.Grid3x3:
                    lightCount = 9;
                    bufferCount = 16;
                    isUniform = false;
                    mipmapStartIndex = Mipmap6StartIndex;
                    break;
                case KanikamaBakeTargetMonitor.PartitionType.Grid4x3:
                    lightCount = 12;
                    bufferCount = 16;
                    isUniform = false;
                    mipmapStartIndex = Mipmap6StartIndex;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(partitionType), partitionType, null);
            }

            colors = new Color[lightCount];
            colorsBuffer = new Color[bufferCount];
            isInitialized = true;
        }

        /// <summary>
        /// Setup Camera position. Supposed to be called from Editor scripts.
        /// </summary>
        public void Setup()
        {
            var cameraTrans = targetCamera.transform;
            var rendererTrans = monitorRenderer.transform;
            var pos = rendererTrans.position - rendererTrans.forward * cameraSettings.distance;
            cameraTrans.SetPositionAndRotation(pos, rendererTrans.rotation);
            targetCamera.orthographic = true;
            targetCamera.nearClipPlane = cameraSettings.near;
            targetCamera.farClipPlane = cameraSettings.far;
            var bounds = GetUnRotatedBounds(monitorRenderer);
            targetCamera.orthographicSize = bounds.extents.y;
            var extents = bounds.extents;
            aspectRatio = extents.x / extents.y;
        }

        static Color Convert(UShort4 uShort4)
        {
            return new Color(Mathf.HalfToFloat(uShort4.x), Mathf.HalfToFloat(uShort4.y), Mathf.HalfToFloat(uShort4.z));
        }

        void UpdateColors()
        {
            readingTexture.ReadPixels(new Rect(0, 0, 256, 256), 0, 0, true);
            var rawData = readingTexture.GetRawTextureData<UShort4>();


            if (isUniform)
            {
                rawData = rawData.GetSubArray(mipmapStartIndex, lightCount);
                for (var i = 0; i < lightCount; i++)
                {
                    colors[i] = Convert(rawData[i]);
                }
            }
            else
            {
                rawData = rawData.GetSubArray(mipmapStartIndex, bufferCount);
                for (var i = 0; i < bufferCount; i++)
                {
                    colorsBuffer[i] = Convert(rawData[i]);
                }

                switch (partitionType)
                {
                    case KanikamaBakeTargetMonitor.PartitionType.Grid3x2:
                        colors[0] = (colorsBuffer[0] + colorsBuffer[4]) * 0.5f;
                        colors[1] = (colorsBuffer[1] + colorsBuffer[2] + colorsBuffer[5] + colorsBuffer[6]) * 0.25f;
                        colors[2] = (colorsBuffer[3] + colorsBuffer[7]) * 0.5f;

                        colors[3] = (colorsBuffer[8] + colorsBuffer[12]) * 0.5f;
                        colors[4] = (colorsBuffer[9] + colorsBuffer[10] + colorsBuffer[13] + colorsBuffer[14]) * 0.25f;
                        colors[5] = (colorsBuffer[11] + colorsBuffer[15]) * 0.5f;
                        break;
                    case KanikamaBakeTargetMonitor.PartitionType.Grid3x3:
                        colors[0] = colorsBuffer[0];
                        colors[1] = (colorsBuffer[1] + colorsBuffer[2]) * 0.5f;
                        colors[2] = colorsBuffer[3];

                        colors[3] = (colorsBuffer[4] + colorsBuffer[8]) * 0.5f;
                        colors[4] = (colorsBuffer[5] + colorsBuffer[6] + colorsBuffer[9] + colorsBuffer[10]) * 0.25f;
                        colors[5] = (colorsBuffer[7] + colorsBuffer[11]) * 0.5f;

                        colors[6] = colorsBuffer[12];
                        colors[7] = (colorsBuffer[13] + colorsBuffer[14]) * 0.5f;
                        colors[8] = colorsBuffer[15];
                        break;
                    case KanikamaBakeTargetMonitor.PartitionType.Grid4x3:
                        colors[0] = colorsBuffer[0];
                        colors[1] = colorsBuffer[1];
                        colors[2] = colorsBuffer[2];
                        colors[3] = colorsBuffer[3];

                        colors[4] = (colorsBuffer[4] + colorsBuffer[8]) * 0.5f;
                        colors[5] = (colorsBuffer[5] + colorsBuffer[9]) * 0.5f;
                        colors[6] = (colorsBuffer[6] + colorsBuffer[10]) * 0.5f;
                        colors[7] = (colorsBuffer[7] + colorsBuffer[11]) * 0.5f;

                        colors[8] = colorsBuffer[12];
                        colors[9] = colorsBuffer[13];
                        colors[10] = colorsBuffer[14];
                        colors[11] = colorsBuffer[15];
                        break;
                    case KanikamaBakeTargetMonitor.PartitionType.Grid1x1:
                    case KanikamaBakeTargetMonitor.PartitionType.Grid2x2:
                    case KanikamaBakeTargetMonitor.PartitionType.Grid4x4:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        static Bounds GetUnRotatedBounds(Renderer renderer)
        {
            var t = renderer.transform;
            var rotation = t.rotation;
            t.rotation = Quaternion.identity;
            var bounds = renderer.bounds;
            t.rotation = rotation;
            return bounds;
        }

        struct UShort4
        {
            public ushort x, y, z, w;
        }
    }
}