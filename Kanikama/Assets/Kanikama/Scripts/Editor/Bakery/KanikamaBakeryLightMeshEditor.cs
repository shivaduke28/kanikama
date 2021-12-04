#if BAKERY_INCLUDED
using UnityEditor;
using Kanikama.Bakery;
using UnityEngine;

namespace Kanikama.Editor.Bakery
{
    [CustomEditor(typeof(KanikamaBakeryLightMesh))]
    public class KanikamaBakeryLightMeshEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var lightMesh = (KanikamaBakeryLightMesh)target;
            var renderer = lightMesh.GetSource();
            if (renderer.sharedMaterials.Length > 1)
            {
                EditorGUILayout.HelpBox("KanikamaBakeryLightMesh does not support Renderer with more than one material slots.", MessageType.Error);
            }
            var material = renderer.sharedMaterial;

            if (!KanikamaLightMaterial.IsBakedEmissive(material))
            {
                EditorGUILayout.HelpBox($"The lightmap flags of the material does not have \"BakedEmissive\".", MessageType.Error);
                if (GUILayout.Button("Fix"))
                {
                    KanikamaLightMaterial.AddBakedEmissiveFlag(material);
                }
            }

            if (!material.HasProperty(KanikamaLightMaterial.ShaderPropertyEmissionColor))
            {
                EditorGUILayout.HelpBox($"Material should has \"{KanikamaLightMaterial.ShaderPropertyEmissionColorName}\" property.", MessageType.Warning);
            }
        }
    }
}
#endif