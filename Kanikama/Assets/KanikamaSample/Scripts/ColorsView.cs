using Kanikama.Udon;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Kanikama.Sample
{
    public class ColorsView : UdonSharpBehaviour
    {
        [SerializeField] KanikamaColorSampler colorSampler;
        [SerializeField] RawImage[] rawImages;

        Color[] colors;

        void Start()
        {
            colors = colorSampler.GetColors();
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