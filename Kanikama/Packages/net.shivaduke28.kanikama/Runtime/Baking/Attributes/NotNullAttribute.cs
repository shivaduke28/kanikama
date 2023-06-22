using System;
using UnityEngine;

namespace Kanikama.Baking.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NonNullAttribute : PropertyAttribute
    {
    }
}
