#if UNITY_EDITOR && !COMPILER_UDONSHARP

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;
using UdonSharpEditor;
using VRC.Udon;

namespace FakeGI
{

    public class LightMapUpdatorHelper : MonoBehaviour
    {
        [SerializeField] private UdonBehaviour youtUdonSharpObject;

        [ContextMenu("Name to Ids")]
        private void NameToIds()
        {
            var updator = (LightMapUpdator)UdonSharpEditorUtility.GetProxyBehaviour(youtUdonSharpObject);
            updator.ConvertPropertyNameToIds();
        }
    }
}
#endif