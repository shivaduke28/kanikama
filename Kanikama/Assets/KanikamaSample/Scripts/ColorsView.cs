using Kanikama.GI.Udon;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Kanikama.Sample
{
    public class ColorsView : UdonSharpBehaviour
    {
        [SerializeField] KanikamaUdonMonitorCamera kanikamaMonitorCamera;
        [SerializeField] RawImage[] rawImages;

        Color[] colors;

        void Start()
        {
            colors = kanikamaMonitorCamera.GetLinearColors();
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