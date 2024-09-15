using System;
using System.Linq;
using Kanikama.Attributes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Kanikama.Components
{
    public class KanikamaMonitorGroup : LightSourceGroupV2
    {
        [Header("Runtime")] [SerializeField] bool isSrp;
        [SerializeField, NonNull] Camera captureCamera;
        [SerializeField, NonNull] Renderer captureTargetQuad;
        [SerializeField] CameraSettings cameraSettings;
        [SerializeField, NonNull] Texture2D readingTexture;
        [SerializeField] Color[] colors;

        [Header("Bake")] [SerializeField] KanikamaMonitorV2[] monitors;
        [SerializeField, NonNull] LightSourceV2 gridCellPrefab;
        [SerializeField] KanikamaMonitorV2.PartitionType partitionType = KanikamaMonitorV2.PartitionType.Grid1x1;
        [SerializeField] MonitorGridFiber[] monitorGridFibers;

        #region Runtime

        [Serializable]
        class CameraSettings
        {
            public float near = 0f;
            public float far = 0.2f;
            public float distance = 0.1f;
            public float aspectRatio = 1f;
        }

        Color[] colorsBuffer;

        int lightCount;
        int bufferCount;
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
            if (camera == captureCamera)
            {
                UpdateColors();
            }
        }

        void OnPostRenderCallback(Camera camera)
        {
            if (camera == captureCamera)
            {
                UpdateColors();
            }
        }

        void OnValidate()
        {
            if (captureCamera == null)
            {
                captureCamera = GetComponent<Camera>();
            }
        }

        void Start()
        {
            if (!isInitialized) Initialize();
        }

        void OnEnable()
        {
            if (isSrp)
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
            if (isSrp)
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
            captureCamera.aspect = cameraSettings.aspectRatio;
            switch (partitionType)
            {
                case KanikamaMonitorV2.PartitionType.Grid1x1:
                    lightCount = 1;
                    bufferCount = lightCount;
                    mipmapStartIndex = Mipmap8StartIndex;
                    break;
                case KanikamaMonitorV2.PartitionType.Grid2x2:
                    lightCount = 4;
                    bufferCount = lightCount;
                    mipmapStartIndex = Mipmap7StartIndex;
                    break;
                case KanikamaMonitorV2.PartitionType.Grid4x4:
                    lightCount = 16;
                    bufferCount = lightCount;
                    mipmapStartIndex = Mipmap6StartIndex;
                    break;
                case KanikamaMonitorV2.PartitionType.Grid3x2:
                    lightCount = 6;
                    bufferCount = 16;
                    mipmapStartIndex = Mipmap6StartIndex;
                    break;
                case KanikamaMonitorV2.PartitionType.Grid3x3:
                    lightCount = 9;
                    bufferCount = 16;
                    mipmapStartIndex = Mipmap6StartIndex;
                    break;
                case KanikamaMonitorV2.PartitionType.Grid4x3:
                    lightCount = 12;
                    bufferCount = 16;
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
        public void SetupCamera()
        {
            var cameraTrans = captureCamera.transform;
            var rendererTrans = captureTargetQuad.transform;
            var pos = rendererTrans.position - rendererTrans.forward * cameraSettings.distance;
            cameraTrans.SetPositionAndRotation(pos, rendererTrans.rotation);
            captureCamera.orthographic = true;
            captureCamera.nearClipPlane = cameraSettings.near;
            captureCamera.farClipPlane = cameraSettings.far;
            var bounds = GetUnRotatedBounds(captureTargetQuad);
            captureCamera.orthographicSize = bounds.extents.y;
            var extents = bounds.extents;
            cameraSettings.aspectRatio = extents.x / extents.y;
        }

        static Color Convert(UShort4 uShort4)
        {
            return new Color(Mathf.HalfToFloat(uShort4.x), Mathf.HalfToFloat(uShort4.y), Mathf.HalfToFloat(uShort4.z));
        }

        void UpdateColors()
        {
            readingTexture.ReadPixels(new Rect(0, 0, 256, 256), 0, 0, true);
            var rawData = readingTexture.GetRawTextureData<UShort4>();

            if (partitionType is KanikamaMonitorV2.PartitionType.Grid1x1
                or KanikamaMonitorV2.PartitionType.Grid2x2
                or KanikamaMonitorV2.PartitionType.Grid3x3
                or KanikamaMonitorV2.PartitionType.Grid4x4)
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
                    case KanikamaMonitorV2.PartitionType.Grid3x2:
                        colors[0] = (colorsBuffer[0] + colorsBuffer[4]) * 0.5f;
                        colors[1] = (colorsBuffer[1] + colorsBuffer[2] + colorsBuffer[5] + colorsBuffer[6]) * 0.25f;
                        colors[2] = (colorsBuffer[3] + colorsBuffer[7]) * 0.5f;

                        colors[3] = (colorsBuffer[8] + colorsBuffer[12]) * 0.5f;
                        colors[4] = (colorsBuffer[9] + colorsBuffer[10] + colorsBuffer[13] + colorsBuffer[14]) * 0.25f;
                        colors[5] = (colorsBuffer[11] + colorsBuffer[15]) * 0.5f;
                        break;
                    case KanikamaMonitorV2.PartitionType.Grid3x3:
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
                    case KanikamaMonitorV2.PartitionType.Grid4x3:
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
                    case KanikamaMonitorV2.PartitionType.Grid1x1:
                    case KanikamaMonitorV2.PartitionType.Grid2x2:
                    case KanikamaMonitorV2.PartitionType.Grid4x4:
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

        #endregion


        #region Bake

        public override ILightSourceV2[] GetAll() => monitorGridFibers.Cast<ILightSourceV2>().ToArray();
        public override ILightSourceV2 Get(int index) => monitorGridFibers[index];

        [Serializable]
        class MonitorGridFiber : ILightSourceV2
        {
            [FormerlySerializedAs("bakeTargets")]
            [SerializeField]
            LightSourceV2[] lightSources;

            public MonitorGridFiber(LightSourceV2[] lightSources)
            {
                this.lightSources = lightSources;
            }

            public void Initialize()
            {
                foreach (var bakeTarget in lightSources)
                {
                    bakeTarget.Initialize();
                    bakeTarget.gameObject.SetActive(true);
                }
            }

            public void TurnOff()
            {
                foreach (var bakeTarget in lightSources)
                {
                    bakeTarget.TurnOff();
                }
            }

            public void TurnOn()
            {
                foreach (var bakeTarget in lightSources)
                {
                    bakeTarget.TurnOn();
                }
            }

            public void Clear()
            {
                foreach (var bakeTarget in lightSources)
                {
                    bakeTarget.gameObject.SetActive(false);
                    bakeTarget.Clear();
                }
            }

            public Color GetLinearColor()
            {
                throw new NotSupportedException("MonitorGridFiber can not be used at runtime.");
            }
        }

        /// <summary>
        /// Setup Grid Fibers. Supposed to be called from Editor scripts.
        /// </summary>
        public void SetupGridFibers()
        {
            foreach (var monitor in monitors)
            {
                monitor.SetupLights(partitionType, gridCellPrefab);
            }
            SetupMonitorGridFibers();
        }

        void SetupMonitorGridFibers()
        {
            var part = (int) partitionType;
            var gridCount = Mathf.FloorToInt(part / 10f) * part % 10;
            monitorGridFibers = new MonitorGridFiber[gridCount];
            for (var i = 0; i < gridCount; i++)
            {
                var fiber = new MonitorGridFiber(monitors.Select(x => x.GetGridCell(i)).ToArray());
                monitorGridFibers[i] = fiber;
            }
        }

        #endregion
    }

    public abstract class KanikamaMonitorV2 : MonoBehaviour
    {
        public abstract LightSourceV2 GetGridCell(int index);
        public abstract void SetupLights(PartitionType partitionType, LightSourceV2 gridCellPrefab);

        public enum PartitionType
        {
            Grid1x1 = 11,
            Grid2x2 = 22,
            Grid3x2 = 32,
            Grid3x3 = 33,
            Grid4x3 = 43,
            Grid4x4 = 44,
        }
    }
}
