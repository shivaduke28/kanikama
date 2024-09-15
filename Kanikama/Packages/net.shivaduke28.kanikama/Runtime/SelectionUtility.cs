using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Kanikama
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
