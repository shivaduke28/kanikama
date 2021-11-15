﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kanikama
{
    public abstract class KanikamaLight : KanikamaLightSource<Light> { }
    public abstract class KanikamaRenderer : KanikamaLightSource<Renderer> { }
}