using UdonSharp;
using UdonSharpEditor;
using VRC.Udon;
using UnityEngine;

namespace Kanikama.Editor
{
    public static class UdonUtil
    {
        public static T FindUdonSharpOfType<T>() where T : UdonSharpBehaviour
        {
            var udonBehaviours = GameObject.FindObjectsOfType<UdonBehaviour>();
            foreach (var udon in udonBehaviours)
            {
                if (!UdonSharpEditorUtility.IsUdonSharpBehaviour(udon)) continue;

                var proxy = UdonSharpEditorUtility.GetProxyBehaviour(udon);
                if (proxy is T result)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
