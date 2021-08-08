using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor
{
    class UtilityWindow : EditorWindow
    {
        TextureGenerator.Parameter texParam;
        RenderTextureDescriptor rtDescriptor;
        Vector2 scrollPosition = new Vector2(0, 0);

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
            Show();
        }

        void DrawTextureGenerater()
        {
            GUILayout.Label("Texture", EditorStyles.boldLabel);
            var indentLevel = EditorGUI.indentLevel;
            using (new EditorGUI.IndentLevelScope(indentLevel + 1))
            {
                if (texParam is null)
                {
                    texParam = new TextureGenerator.Parameter();
                }
                texParam.width = EditorGUILayout.IntField("width", texParam.width);
                texParam.height = EditorGUILayout.IntField("height", texParam.height);
                texParam.format = (TextureFormat)EditorGUILayout.EnumPopup("format", texParam.format);
                texParam.extension = (TextureGenerator.TextureExtension)EditorGUILayout.EnumPopup("ext", texParam.extension);
                texParam.mipChain = EditorGUILayout.Toggle("mipChain", texParam.mipChain);
                texParam.linear = EditorGUILayout.Toggle("linear", texParam.linear);
                texParam.isReadable = EditorGUILayout.Toggle("is readable", texParam.isReadable);
                if (GUILayout.Button("Create"))
                {
                    var tex = TextureGenerator.GenerateTexture("Assets", "texture", texParam);
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
