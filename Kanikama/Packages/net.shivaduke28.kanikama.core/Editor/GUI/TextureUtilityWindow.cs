using Kanikama.Core.Editor.Util;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Core.Editor.GUI
{
    internal sealed class TextureUtilityWindow : EditorWindow
    {
        [SerializeField] TextureParameter textureParameter;
        [SerializeField] TextureImportParameter textureImportParameter;

        Vector2 scrollPosition = new Vector2(0, 0);
        SerializedObject serializedObject;
        SerializedProperty textureParameterProperty;
        SerializedProperty textureImportParameterProperty;

        [MenuItem("Window/Kanikama/Texture Utility Window")]
        static void ShowWindow()
        {
            var window = (TextureUtilityWindow) GetWindow(typeof(TextureUtilityWindow));
            window.Initialize();
        }

        void Initialize()
        {
            titleContent.text = "Kanikama Texture Utility";
            serializedObject = new SerializedObject(this);
            textureParameterProperty = serializedObject.FindProperty(nameof(textureParameter));
            textureImportParameterProperty = serializedObject.FindProperty(nameof(textureImportParameter));
            Show();
        }

        void OnGUI()
        {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;
                DrawTextureGenerator();
            }
        }

        void DrawTextureGenerator()
        {
            GUILayout.Label("Texture", EditorStyles.boldLabel);
            var indentLevel = EditorGUI.indentLevel;
            using (new EditorGUI.IndentLevelScope(indentLevel + 1))
            {
                EditorGUILayout.PropertyField(textureParameterProperty);
                EditorGUILayout.PropertyField(textureImportParameterProperty);
                if (GUILayout.Button("Create"))
                {
                    serializedObject.ApplyModifiedProperties();
                    var tex = TextureUtility.GenerateTexture(textureParameter);
                    TextureUtility.SaveTexture2D(tex, "Assets", "texture", textureImportParameter);
                    Selection.activeObject = tex;
                }
            }
        }
    }
}
