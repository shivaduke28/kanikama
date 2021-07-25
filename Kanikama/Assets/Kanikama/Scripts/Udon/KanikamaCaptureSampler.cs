using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;

namespace Kanikama.Udon
{
    public class KanikamaCaptureSampler : UdonSharpBehaviour
    {
        [SerializeField] private Texture2D tex;
        [SerializeField] private int partitionType;
        public float intensity = 1f;

        private int lightCount;
        private int mipmapLevel;
        private bool isUniform;
        private bool isInitialized;

        private Color[] colors;

        private void Start()
        {
            if (!isInitialized) Initialize();
        }


        public Color[] GetColors()
        {
            if (!isInitialized) Initialize();
            return colors;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            tex.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
            tex.Apply();
            var pixels = tex.GetPixels(mipmapLevel);
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
                    case 23:
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
                    case 34:
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

        private void Initialize()
        {
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
                case 23:
                case 33:
                case 34:
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