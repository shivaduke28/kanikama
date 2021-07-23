using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;

namespace Kanikama.Udon
{
    public class KanikamaMonitorComposite : UdonSharpBehaviour
    {
        [SerializeField] Material[] materials;
        [SerializeField] Texture2D tex;
        [SerializeField] int partitionType;
        public float intensity = 1f;

        int lightCount;
        int mipmapLevel;
        int countX;
        int countY;
        float[] intensities;
        Color[] colors;
        bool isUniform;

        void Start()
        {
            Initialize();
            intensities = new float[lightCount];
            colors = new Color[lightCount];
        }


        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {

            tex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            tex.Apply();
            if (isUniform)
            {
                colors = tex.GetPixels(mipmapLevel);
            }
            else
            {
                var pixels = tex.GetPixels(mipmapLevel);
                switch (partitionType)
                {
                    case 23:
                        colors[0] = (pixels[0] + pixels[4]) * 0.5f;
                        colors[1] = (pixels[1] + pixels[2] + pixels[5] + pixels[6]) * 0.25f;
                        colors[2] = (pixels[3] + pixels[7]) * 0.5f;

                        colors[3] = (pixels[8] + pixels[12]) * 0.5f;
                        colors[4] = (pixels[9] + pixels[10] + pixels[13] + pixels[14]) * 0.25f;
                        colors[5] = (pixels[11] + pixels[15]) * 0.5f;
                        break;
                    case 33:
                        colors[0] = pixels[0];
                        colors[1] = (pixels[1] + pixels[2]) * 0.5f;
                        colors[2] = pixels[3];

                        colors[3] = (pixels[4] + pixels[8]) * 0.5f;
                        colors[4] = (pixels[5] + pixels[6] + pixels[9] + pixels[10]) * 0.25f;
                        colors[5] = (pixels[7] + pixels[11]) * 0.5f;

                        colors[6] = pixels[12];
                        colors[7] = (pixels[13] + pixels[14]) * 0.5f;
                        colors[8] = pixels[15];
                        break;
                    case 34:
                        colors[0] = pixels[0];
                        colors[1] = pixels[1];
                        colors[2] = pixels[2];
                        colors[3] = pixels[3];

                        colors[4] = (pixels[4] + pixels[8]) * 0.5f;
                        colors[5] = (pixels[5] + pixels[9]) * 0.5f;
                        colors[6] = (pixels[6] + pixels[10]) * 0.5f;
                        colors[7] = (pixels[7] + pixels[11]) * 0.5f;

                        colors[8] = pixels[8];
                        colors[9] = pixels[9];
                        colors[10] = pixels[10];
                        colors[11] = pixels[11];
                        break;
                    default:
                        return;
                }

            }

            for (var i = 0; i < lightCount; i++)
            {
                intensities[i] = intensity;
            }

            foreach (var mat in materials)
            {
                mat.SetColorArray("_Colors", colors);
                mat.SetFloatArray("_Intensities", intensities);
            }
        }

        void Initialize()
        {
            countX = partitionType % 10;
            countY = Mathf.FloorToInt(partitionType / 10);
            lightCount = countX * countY;

            switch (partitionType)
            {
                case 11:
                    mipmapLevel = 8;
                    isUniform = true;
                    return;
                case 22:
                    mipmapLevel = 7;
                    isUniform = true;
                    return;
                case 44:
                    mipmapLevel = 6;
                    isUniform = true;
                    return;
                case 23:
                case 33:
                case 34:
                    mipmapLevel = 6;
                    isUniform = false;
                    return;
                default:
                    Debug.LogError("partionTypeの値が不正です。");
                    return;
            }
        }

    }
}