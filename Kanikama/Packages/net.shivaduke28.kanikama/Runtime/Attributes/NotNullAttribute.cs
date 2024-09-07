using System;
using UnityEngine;

namespace Kanikama.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NonNullAttribute : PropertyAttribute
    {
    }
}
