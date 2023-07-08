using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon.AudioLink
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public sealed class KanikamaUdonAudioLinkLightSource : KanikamaUdonLightSource
    {
        [SerializeField] VRCAudioLink.AudioLink audioLink;
        [SerializeField] int band;
        [SerializeField] int delay;

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
                return audioData[dataIndex].linear;
            }
            else
            {
                return Color.black;
            }
        }
    }
}
