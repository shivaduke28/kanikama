using Kanikama.Udon;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Kanikama.Sample
{
    public class ColorsView : UdonSharpBehaviour
    {
        [SerializeField] KanikamaCamera kanikamaCamera;
        [SerializeField] RawImage[] rawImages;

        Color[] colors;

        void Start()
        {
            colors = kanikamaCamera.GetColors();
        }
        void LateUpdate()
        {
            for (var i = 0; i < colors.Length; i++)
            {
                rawImages[i].color = colors[i];
            }
        }
    }
}