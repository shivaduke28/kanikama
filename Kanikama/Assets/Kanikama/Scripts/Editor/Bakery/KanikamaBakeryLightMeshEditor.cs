#if BAKERY_INCLUDED
using UnityEditor;
using Kanikama.Bakery;

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

            if (!material.IsKeywordEnabled(KanikamaLightMaterial.ShaderKeywordEmission))
            {
                EditorGUILayout.HelpBox($"Shader keyword \"{KanikamaLightMaterial.ShaderKeywordEmission}\" should be enabled.", MessageType.Warning);
            }

            if (!material.HasProperty(KanikamaLightMaterial.ShaderPropertyEmissionColor))
            {
                EditorGUILayout.HelpBox($"Material should has \"{KanikamaLightMaterial.ShaderPropertyEmissionColorName}\" property.", MessageType.Warning);
            }
        }
    }
}
#endif