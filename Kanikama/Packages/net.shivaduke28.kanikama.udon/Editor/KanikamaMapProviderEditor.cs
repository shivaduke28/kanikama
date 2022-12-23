using Kanikama.Baking;
using System.Linq;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using Kanikama.Editor;

namespace Kanikama.Udon.Editor
{
    [CustomEditor(typeof(KanikamaMapProvider))]
    public class KanikamaMapProviderEditor : UnityEditor.Editor
    {
        UdonSharpBehaviour proxy;
        SerializedProperty compositeTexturesProperty;
        SerializedProperty compositeMaterialsProperty;

        void OnEnable()
        {
            if (target == null) return;
            proxy = (UdonSharpBehaviour)target;
            compositeTexturesProperty = serializedObject.FindProperty("compositeTextures");
            compositeMaterialsProperty = serializedObject.FindProperty("compositeMaterials");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpEditorUtility.IsProxyBehaviour(proxy))
            {
                base.OnInspectorGUI();

                EditorGUILayout.Space();
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                EditorGUILayout.Space();
                if (GUILayout.Button($"Setup by {nameof(KanikamaSettings)} asset (CRT)"))
                {
                    Setup(true);
                }
                EditorGUILayout.Space();
                if (GUILayout.Button($"Setup by {nameof(KanikamaSettings)} asset (RT)"))
                {
                    Setup(false);
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                if (UdonSharpGUI.DrawConvertToUdonBehaviourButton(target))
                {
                    return;
                }
            }
        }

        void Setup(bool crt)
        {
            var sceneAsset = KanikamaEditorUtility.GetActiveSceneAsset();
            if (sceneAsset == null)
            {
                Debug.LogError("[Kanikama] This scene is not saved yet.");
                return;
            }
            var kanikamaSettings = KanikamaSettings.FindSettings(sceneAsset);
            if (kanikamaSettings == null)
            {
                Debug.LogError("[Kanikama] KanikamaSettings asset is not found.");
                return;
            }

            var bakedAsset = kanikamaSettings.bakedAsset;
            var textures = crt ? bakedAsset.customRenderTextures.Select(x => (RenderTexture)x).ToList() : bakedAsset.renderTextures;
            var arrayCount = textures.Count;
            compositeTexturesProperty.arraySize = arrayCount;
            for (var i = 0; i < arrayCount; i++)
            {
                var prop = compositeTexturesProperty.GetArrayElementAtIndex(i);
                prop.objectReferenceValue = textures[i];
            }

            var materials = crt ? bakedAsset.customRenderTextureMaterials : bakedAsset.renderTextureMaterials;
            compositeMaterialsProperty.arraySize = materials.Count;

            for (var i = 0; i < materials.Count; i++)
            {
                var prop = compositeMaterialsProperty.GetArrayElementAtIndex(i);
                prop.objectReferenceValue = materials[i];
            }

            serializedObject.ApplyModifiedProperties();
            UdonSharpEditorUtility.CopyProxyToUdon(proxy);
        }
    }
}