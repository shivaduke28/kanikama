#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System.Collections;
using UdonSharp;
using UnityEngine;
using UdonSharpEditor;
using VRC.Udon;
using System.Linq;
using Kanikama.Udon;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Kanikama.EditorOnly
{
    public class KanikamaMapSetter : MonoBehaviour
    {
        public Texture[] kanikamaMaps;

        [ContextMenu("Update Scene Materials")]
        void UpdateKanikamaRenderers()
        {
            var kanikamaRenderers = FindObjectsOfType<UdonSharpBehaviour>()
                .Select(x => x as KanikamaRenderer)
                .Where(x => x != null).ToList();

            foreach(var kanikamaRenderer in kanikamaRenderers)
            {

                UdonSharpEditorUtility.CopyUdonToProxy(kanikamaRenderer);
                kanikamaRenderer.SetKanikamaMaps(kanikamaMaps);
                UdonSharpEditorUtility.CopyProxyToUdon(kanikamaRenderer);
            }
        }
    }
}
#endif