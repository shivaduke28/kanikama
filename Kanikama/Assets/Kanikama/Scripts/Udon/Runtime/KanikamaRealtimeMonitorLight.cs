using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    // should be attached to KanikamaProvider GameObject.
    [RequireComponent(typeof(Camera)), UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KanikamaRealtimeMonitorLight : UdonSharpBehaviour
    {
        [SerializeField] KanikamaCamera kanikamaCamera;
        [SerializeField] Light light;
        [SerializeField] float intensity = 1;
        [SerializeField] bool weightEnable = false;
        [SerializeField] float[] weights;

        Color[] colors;
        int count;

        void OnEnable()
        {
            colors = kanikamaCamera.GetColors();
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
            var totalWeight = 0f;
            for (var i = 0; i < count; i++)
            {
                var weight = weightEnable ? weights[i] : 1f;
                color += (Color)colors[i] * weight;
                totalWeight += weight;
            }
            color /= totalWeight;
            var max = color.maxColorComponent;

            light.color = color / max;
            light.intensity = max * intensity;
        }
    }
}