using System;
using Kanikama.Attributes;
using UnityEngine;

namespace Kanikama
{
    [RequireComponent(typeof(KanikamaManager))]
    public class KanikamaRealtimeLight : KanikamaBehaviour
    {
        [SerializeField, NonNull] KanikamaManager kanikamaManager;
        [SerializeField, NonNull] new Light light;
        [SerializeField] public float intensity = 1;
        [SerializeField] bool weightEnable = false;
        [SerializeField] float[] weights;

        Vector4[] colors;
        int count;

        void OnValidate()
        {
            if (kanikamaManager == null) kanikamaManager = GetComponent<KanikamaManager>();
        }

        void Start()
        {
            colors = kanikamaManager.GetColors();
            count = colors.Length;
            if (count == 0)
            {
                enabled = false;
            }
            if (weightEnable && weights.Length != count)
            {
                var newWeights = new float[count];
                var i = 0;
                for (; i < weights.Length && i < count; i++)
                {
                    newWeights[i] = weights[i];
                }
                for (; i < count; i++)
                {
                    newWeights[i] = 1;
                }
                weights = newWeights;
            }
        }

        // Note:
        // Colors are updated by KanikamaGIUpdater on OnPreCull.
        void OnPreRender()
        {
            var color = Color.black;
            var totalWeight = 0f;
            for (var i = 0; i < count; i++)
            {
                var weight = weightEnable ? weights[i] : 1f;
                color += (Color) colors[i] * weight;
                totalWeight += weight;
            }
            color /= totalWeight;
            var max = color.maxColorComponent;

            light.color = color / max;
            light.intensity = max * intensity;
        }
    }
}
