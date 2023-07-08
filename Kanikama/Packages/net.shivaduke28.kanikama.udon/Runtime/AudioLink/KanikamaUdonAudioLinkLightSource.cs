﻿using UdonSharp;
using UnityEngine;

namespace Kanikama.Udon.AudioLink
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public sealed class KanikamaUdonAudioLinkLightSource : KanikamaUdonLightSource
    {
        [SerializeField] VRCAudioLink.AudioLink audioLink;
        [SerializeField] int band;
        [SerializeField] int delay;
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
