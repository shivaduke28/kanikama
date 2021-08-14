
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Kanikama.Udon;
using UnityEngine.UI;

namespace Kanikama.Sample
{
    public class ColorsView : UdonSharpBehaviour
    {
        [SerializeField] KanikamaColorSampler colorSampler;
        [SerializeField] RawImage[] rawImages;

        Color[] colors;

        private void Start()
        {
            colors = colorSampler.GetColors();
        }

        private void Update()
        {
            for (var i = 0; i < colors.Length; i++)
            {
                rawImages[i].color = colors[i];
            }
        }
    }
}