using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon.AudioLink
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public sealed class KanikamaUdonAudioLinkLightSource : KanikamaUdonLightSource
    {
        [SerializeField] global::AudioLink.AudioLink audioLink;
        [SerializeField, Range(0, 63)] int band;
        [SerializeField, Range(0, 127)] int delay;
        [SerializeField] float intensity = 1f;

        int dataIndex;

        void Start()
        {
            dataIndex = band * 128 + delay;
        }

        public override Color GetLinearColor()
        {
            var audioData = audioLink.audioData;
            if (audioData.Length != 0)
            {
                return audioData[dataIndex].linear * intensity;
            }
            else
            {
                return Color.black;
            }
        }
    }
}
