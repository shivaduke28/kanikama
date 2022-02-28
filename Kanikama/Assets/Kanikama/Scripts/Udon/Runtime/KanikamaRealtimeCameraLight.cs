using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon
{
    // should be attached to KanikamaProvider GameObject.
    [RequireComponent(typeof(Camera)), UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KanikamaRealtimeCameraLight : UdonSharpBehaviour
    {
        [SerializeField] KanikamaCamera kanikamaCamera;
        [SerializeField] Light light;
        [SerializeField] public float intensity = 1;

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
        // Colors are updated by KanikamaCamera on OnPostRender of a Camera capturing a monitor.
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