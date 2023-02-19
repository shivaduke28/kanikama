﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.GI
{
    public sealed class KanikamaSceneDescriptor : MonoBehaviour
    {
        [SerializeField] LightSource[] lightSources;
        [SerializeField] LightSourceGroup[] lightSourceGroups;

        public ILightSourceHandle[] GetLightSources()
        {
            var list = new List<ILightSourceHandle>();
            list.AddRange(lightSources.Select(x => x.GetHandle()));
            list.AddRange(lightSourceGroups.SelectMany(x => x.GetHandles()));
            return list.ToArray();
        }
    }
}
