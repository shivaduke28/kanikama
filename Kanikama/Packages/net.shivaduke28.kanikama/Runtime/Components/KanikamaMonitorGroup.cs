using System.Collections.Generic;
using Kanikama.Attributes;
using UnityEngine;

namespace Kanikama.Components
{
    [RequireComponent(typeof(KanikamaMonitorGroupHolder))]
    public class KanikamaMonitorGroup : KanikamaLightSourceGroup
    {
#if !COMPILER_UDONSHARP && UNITY_EDITOR

        public override List<IKanikamaBakeTarget> GetAll() => GetComponent<KanikamaMonitorGroupHolder>().GetAll();

        public override IKanikamaBakeTarget Get(int index) => GetComponent<KanikamaMonitorGroupHolder>().Get(index);


        public void Setup()
        {
            // baking
            GetComponent<KanikamaMonitorGroupHolder>().Setup(partitionType);

            // runtime
            camera.orthographic = true;
            var cameraTrans = camera.transform;
            var rendererTrans = cameraTargetQuad.transform;
            var pos = rendererTrans.position - rendererTrans.forward * cameraDistance;
            cameraTrans.SetPositionAndRotation(pos, rendererTrans.rotation);
            camera.nearClipPlane = cameraNear;
            camera.farClipPlane = cameraFar;
            var bounds = GetUnRotatedBounds(cameraTargetQuad);
            camera.orthographicSize = bounds.extents.y;
            var extents = bounds.extents;
            aspectRatio = extents.x / extents.y;
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
#endif
        [Header("Runtime")]
        [SerializeField, NonNull]
        Renderer cameraTargetQuad;

        [SerializeField] KanikamaMonitorPartitionType partitionType = KanikamaMonitorPartitionType.Grid1x1;

        [SerializeField, NonNull] Texture2D readingTexture;
        [SerializeField] float cameraNear = 0f;
        [SerializeField] float cameraFar = 0.2f;
        [SerializeField] float cameraDistance = 0.1f;
        [SerializeField, NonNull] new Camera camera;
        [SerializeField] float aspectRatio = 1f;
        public float intensity = 1f;

        [ColorUsage(false, true), SerializeField]
        Color[] colors = new Color[0];

        int lightCount;
        int mipmapLevel;
        bool isUniform;
        bool isInitialized;

        void OnValidate()
        {
            if (camera == null)
            {
                camera = GetComponent<Camera>();
            }
        }

        void Start()
        {
            if (!isInitialized) Initialize();
        }

        public override Color[] GetLinearColors()
        {
            if (!isInitialized) Initialize();
            return colors;
        }

        void OnPostRender()
        {
            // Note:
            // pixel colors are linear if so is the source render texture (maybe)
            // and HDR if so is the reading texture
            readingTexture.ReadPixels(new Rect(0, 0, 256, 256), 0, 0, true);

            // Note: call Apply() here if you want update readingTexture,
            //       is useful for debugging mipmapped textures in Editor.

            // readingTexture.Apply();

            var pixels = readingTexture.GetPixels(mipmapLevel);
            if (isUniform)
            {
                for (var i = 0; i < lightCount; i++)
                {
                    colors[i] = pixels[i] * intensity;
                }
            }
            else
            {
                switch (partitionType)
                {
                    case KanikamaMonitorPartitionType.Grid3x2:
                        colors[0] = (pixels[0] + pixels[4]) * 0.5f * intensity;
                        colors[1] = (pixels[1] + pixels[2] + pixels[5] + pixels[6]) * 0.25f * intensity;
                        colors[2] = (pixels[3] + pixels[7]) * 0.5f * intensity;

                        colors[3] = (pixels[8] + pixels[12]) * 0.5f * intensity;
                        colors[4] = (pixels[9] + pixels[10] + pixels[13] + pixels[14]) * 0.25f * intensity;
                        colors[5] = (pixels[11] + pixels[15]) * 0.5f * intensity;
                        break;
                    case KanikamaMonitorPartitionType.Grid3x3:
                        colors[0] = pixels[0] * intensity;
                        colors[1] = (pixels[1] + pixels[2]) * 0.5f * intensity;
                        colors[2] = pixels[3] * intensity;

                        colors[3] = (pixels[4] + pixels[8]) * 0.5f * intensity;
                        colors[4] = (pixels[5] + pixels[6] + pixels[9] + pixels[10]) * 0.25f * intensity;
                        colors[5] = (pixels[7] + pixels[11]) * 0.5f * intensity;

                        colors[6] = pixels[12] * intensity;
                        colors[7] = (pixels[13] + pixels[14]) * 0.5f * intensity;
                        colors[8] = pixels[15] * intensity;
                        break;
                    case KanikamaMonitorPartitionType.Grid4x3:
                        colors[0] = pixels[0] * intensity;
                        colors[1] = pixels[1] * intensity;
                        colors[2] = pixels[2] * intensity;
                        colors[3] = pixels[3] * intensity;

                        colors[4] = (pixels[4] + pixels[8]) * 0.5f * intensity;
                        colors[5] = (pixels[5] + pixels[9]) * 0.5f * intensity;
                        colors[6] = (pixels[6] + pixels[10]) * 0.5f * intensity;
                        colors[7] = (pixels[7] + pixels[11]) * 0.5f * intensity;

                        colors[8] = pixels[12] * intensity;
                        colors[9] = pixels[13] * intensity;
                        colors[10] = pixels[14] * intensity;
                        colors[11] = pixels[15] * intensity;
                        break;
                    default:
                        return;
                }
            }
        }

        void Initialize()
        {
            camera.aspect = aspectRatio;

            switch (partitionType)
            {
                case KanikamaMonitorPartitionType.Grid1x1:
                    isUniform = true;
                    lightCount = 1;
                    mipmapLevel = 8;
                    break;
                case KanikamaMonitorPartitionType.Grid2x2:
                    isUniform = true;
                    lightCount = 4;
                    mipmapLevel = 7;
                    break;
                case KanikamaMonitorPartitionType.Grid4x4:
                    isUniform = true;
                    lightCount = 16;
                    mipmapLevel = 6;
                    break;
                case KanikamaMonitorPartitionType.Grid3x2:
                    lightCount = 6;
                    isUniform = false;
                    mipmapLevel = 6;
                    break;
                case KanikamaMonitorPartitionType.Grid3x3:
                    lightCount = 9;
                    isUniform = false;
                    mipmapLevel = 6;
                    break;
                case KanikamaMonitorPartitionType.Grid4x3:
                    lightCount = 12;
                    isUniform = false;
                    mipmapLevel = 6;
                    break;
                default:
                    return;
            }
            colors = new Color[lightCount];
            isInitialized = true;
        }
    }
}
