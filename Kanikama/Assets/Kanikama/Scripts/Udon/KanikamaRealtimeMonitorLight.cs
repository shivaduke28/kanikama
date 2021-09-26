using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    // should be attached to KanikamaProvider GameObject.
    [RequireComponent(typeof(Camera))]
    public class KanikamaRealtimeMonitorLight : UdonSharpBehaviour
    {
        [SerializeField] KanikamaColorSampler monitorColorSampler;
        [SerializeField] Light light;
        [SerializeField] float intensity = 1;

        Color[] colors;
        int count;

        void OnEnable()
        {
            colors = monitorColorSampler.GetColors();
            count = colors.Length;
            if (count == 0)
            {
                enabled = false;
            }
        }

        // Note:
        // Colors are updated by KanikamaColorCollector on OnPreCull.
        void OnPreRender()
        {
            var color = Color.black;
            for (var i = 0; i < count; i++)
            {
                color += colors[i];
            }
            color /= count;
            var max = color.maxColorComponent;

            light.color = color / max;
            light.intensity = max * intensity;
        }
    }
}