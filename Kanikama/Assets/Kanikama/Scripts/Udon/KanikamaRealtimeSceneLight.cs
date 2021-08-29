using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    // should be attached to KanikamaProvider GameObject.
    public class KanikamaRealtimeSceneLight : UdonSharpBehaviour
    {
        [SerializeField] KanikamaColorCollector colorCollector;
        [SerializeField] Light light;
        [SerializeField] float intensity = 1;

        Vector4[] colors;
        int count;

        void OnEnable()
        {
            colors = colorCollector.GetColors();
            count = colors.Length;
            if (count == 0)
            {
                enabled = false;
            }
        }

        // Note:
        // OnPreRender is called after OnPreCull and
        // colorCollector.Collect() will be called in OnPreCull of KanikamaProvider,
        // so it is skippped here for optimization.
        void OnPreRender()
        {
            var color = Color.black;
            for (var i = 0; i < count; i++)
            {
                color += (Color)colors[i];
            }
            color /= count;
            var max = color.maxColorComponent;

            light.color = color / max;
            light.intensity = max * intensity;
        }
    }
}