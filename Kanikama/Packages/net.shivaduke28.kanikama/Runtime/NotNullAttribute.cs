using System;
using UnityEngine;

namespace Kanikama
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NonNullAttribute : PropertyAttribute
    {
    }
}
