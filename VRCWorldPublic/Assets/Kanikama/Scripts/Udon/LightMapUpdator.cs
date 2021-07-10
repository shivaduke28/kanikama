using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;

namespace FakeGI
{
    public class LightMapUpdator : UdonSharpBehaviour
    {
        [SerializeField] private Material mat;

        [SerializeField] private int lightMapsId;
        [SerializeField] private int colorsId;
        [SerializeField] private int intensitiesId;

        [SerializeField] private Light[] lights;
        [SerializeField] private float[] intensities;
        [SerializeField] private Color[] colors;
        [SerializeField] private Texture[] lightMaps;
        [SerializeField] private Object _texArray;


        private int lightCount;

        private void Start()
        {
            lightCount = lights.Length;
            intensities = new float[lightCount];
            colors = new Color[lightCount];
            mat.SetTexture("_MyArr", (Texture2DArray)_texArray);
        }

        private void Update()
        {
            for (var i = 0; i < lightCount; i++)
            {
                var light = lights[i];
                colors[i] = light.color;
                intensities[i] = light.intensity;
            }

            mat.SetColorArray("_Colors", colors);
            mat.SetFloatArray("_Intensities", intensities);
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public void ConvertPropertyNameToIds()
        {
            intensitiesId = Shader.PropertyToID("_Intensities");
            colorsId = Shader.PropertyToID("_Colors");
            UdonSharpEditor.UdonSharpEditorUtility.CopyProxyToUdon(this);
        }
#endif
    }
}