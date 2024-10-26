#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Kanikama.Utility
{
    public static class SelectionUtility
    {
        public static void SetActiveObject(Object obj)
        {
#if UNITY_EDITOR
            Selection.activeObject = obj;
#endif
        }
    }
}
