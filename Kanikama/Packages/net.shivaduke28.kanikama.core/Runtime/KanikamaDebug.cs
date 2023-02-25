using System;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Core
{
    public static class KanikamaDebug
    {
        const string Format = "[Kanikama] {0}";

        public static void Log(string message) => Debug.LogFormat(Format, message);

        public static void LogError(string message) => Debug.LogErrorFormat(Format, message);

        public static void LogException(Exception e) => Debug.LogErrorFormat(Format, e);
    }
}