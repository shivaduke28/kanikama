﻿using UnityEngine;

namespace Kanikama.GI.Runtime
{
    public interface ILightSource
    {
        Color GetLinearColor();
    }

    public abstract class LightSource : MonoBehaviour, ILightSource
    {
        public abstract Color GetLinearColor();
    }
}
