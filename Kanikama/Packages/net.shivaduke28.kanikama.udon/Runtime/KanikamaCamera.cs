using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    [RequireComponent(typeof(Camera)), UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KanikamaCamera : UdonSharpBehaviour
    {
        [SerializeField] Texture2D readingTexture;
        [SerializeField] int partitionType;
        [SerializeField] Camera camera;
        [SerializeField] float aspectRatio = 1f;
        public float intensity = 1f;
        [ColorUsage(false, true), SerializeField, HideInInspector] Color[] colors;

        int lightCount;
        int mipmapLevel;
        bool isUniform;
        bool isInitialized;

        void Start()
        {
            if (!isInitialized) Initialize();
        }

        public Color[] GetColors()
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
                    case 32:
                        colors[0] = (pixels[0] + pixels[4]) * 0.5f * intensity;
                        colors[1] = (pixels[1] + pixels[2] + pixels[5] + pixels[6]) * 0.25f * intensity;
                        colors[2] = (pixels[3] + pixels[7]) * 0.5f * intensity;

                        colors[3] = (pixels[8] + pixels[12]) * 0.5f * intensity;
                        colors[4] = (pixels[9] + pixels[10] + pixels[13] + pixels[14]) * 0.25f * intensity;
                        colors[5] = (pixels[11] + pixels[15]) * 0.5f * intensity;
                        break;
                    case 33:
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
                    case 43:
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
            var countX = partitionType % 10;
            var countY = Mathf.FloorToInt(partitionType / 10);
            lightCount = countX * countY;
            colors = new Color[lightCount];

            switch (partitionType)
            {
                case 11:
                    mipmapLevel = 8;
                    isUniform = true;
                    break;
                case 22:
                    mipmapLevel = 7;
                    isUniform = true;
                    break;
                case 44:
                    mipmapLevel = 6;
                    isUniform = true;
                    break;
                case 32:
                case 33:
                case 43:
                    mipmapLevel = 6;
                    isUniform = false;
                    break;
                default:
                    Debug.LogError("partionTypeの値が不正です。");
                    return;
            }
            isInitialized = true;
        }
    }
}