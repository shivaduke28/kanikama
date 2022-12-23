using UnityEditor;
using UnityEngine;
using Kanikama.Baking;

namespace Kanikama.Editor
{
    class UtilityWindow : EditorWindow
    {
        [SerializeField] TextureGenerator.Parameter texParam;
        [SerializeField] TextureGenerator.ImportParameter importParam;
        RenderTextureDescriptor rtDescriptor;
        Vector2 scrollPosition = new Vector2(0, 0);
        SerializedObject serializedObject;
        SerializedProperty texParamProperty;
        SerializedProperty importParamProperty;

        [MenuItem("Window/Kanikama/Utility")]
        static void ShowWindow()
        {
            var window = (UtilityWindow)GetWindow(typeof(UtilityWindow));
            window.Initialize();
        }

        void OnGUI()
        {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;
                DrawTextureGenerater();
                EditorGUILayout.Space();
                DrawRenderTextureGenerator();
            }
        }

        public void Initialize()
        {
            titleContent.text = "Kanikama Utility";
            rtDescriptor = new RenderTextureDescriptor(256, 256);
            serializedObject = new SerializedObject(this);
            texParamProperty = serializedObject.FindProperty(nameof(texParam));
            importParamProperty = serializedObject.FindProperty(nameof(importParam));
            Show();
        }

        void DrawTextureGenerater()
        {
            GUILayout.Label("Texture", EditorStyles.boldLabel);
            var indentLevel = EditorGUI.indentLevel;
            using (new EditorGUI.IndentLevelScope(indentLevel + 1))
            {
                EditorGUILayout.PropertyField(texParamProperty);
                EditorGUILayout.PropertyField(importParamProperty);
                if (GUILayout.Button("Create"))
                {
                    serializedObject.ApplyModifiedProperties();
                    var tex = TextureGenerator.GenerateTexture(texParam);
                    TextureGenerator.SaveTexture2D(tex, "Assets", "texture", importParam);
                    Selection.activeObject = tex;
                }
            }
        }

        void DrawRenderTextureGenerator()
        {
            GUILayout.Label("RenderTexture for Capture", EditorStyles.boldLabel);
            var indentLevel = EditorGUI.indentLevel;
            using (new EditorGUI.IndentLevelScope(indentLevel + 1))
            {
                rtDescriptor.width = EditorGUILayout.IntField("width", rtDescriptor.width);
                rtDescriptor.height = EditorGUILayout.IntField("height", rtDescriptor.height);
                rtDescriptor.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
                rtDescriptor.colorFormat = RenderTextureFormat.ARGBHalf;
                rtDescriptor.depthBufferBits = 0;
                rtDescriptor.useDynamicScale = false;
                rtDescriptor.msaaSamples = 1;
                rtDescriptor.volumeDepth = 1;
                if (GUILayout.Button("Generate"))
                {
                    var tex = TextureGenerator.GenerateRenderTexture("Assets", "rt", rtDescriptor);
                    Selection.activeObject = tex;
                }
            }
        }
    }
}
