using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    // should be attached to KanikamaProvider GameObject.
    [RequireComponent(typeof(Camera)), UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
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
        // Colors are updated by KanikamaColorCollector on OnPreCull.
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