using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;

namespace FakeGI
{
    public class LightMapUpdator : UdonSharpBehaviour
    {

        [SerializeField] private Light light1;
        [SerializeField] private Light light2;

        [SerializeField] private Material mat;


        private void Update()
        {
            mat.SetColor("_Color1", light1.color);
            mat.SetFloat("_Intensity1", light1.intensity);
            mat.SetColor("_Color2", light2.color);
            mat.SetFloat("_Intensity2", light2.intensity);
        }
    }
}