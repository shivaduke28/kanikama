using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor
{
    class UtilityWindow : EditorWindow
    {
        TextureGenerator.Parameter texParam = new TextureGenerator.Parameter();
        bool showTextureParam;
        Vector2 scrollPosition = new Vector2(0, 0);

        [MenuItem("Window/Kanikama/Utility")]
        static void Initialize()
        {
            var window = GetWindow(typeof(UtilityWindow));
            window.Show();
        }

        void OnEnable()
        {
            titleContent.text = "Kanikama Utility";
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            showTextureParam = EditorGUILayout.Foldout(showTextureParam, "Texture Generator");
            if (showTextureParam)
            {
                EditorGUI.indentLevel++;
                texParam.width = EditorGUILayout.IntField("width", texParam.width);
                texParam.height = EditorGUILayout.IntField("height", texParam.height);
                texParam.format = (TextureFormat)EditorGUILayout.EnumPopup("format", texParam.format);
                texParam.extension = (TextureGenerator.TextureExtension)EditorGUILayout.EnumPopup("ext", texParam.extension);
                texParam.mipChain = EditorGUILayout.Toggle("mipChain", texParam.mipChain);
                texParam.linear = EditorGUILayout.Toggle("linear", texParam.linear);
                texParam.readWrite = EditorGUILayout.Toggle("read/write", texParam.readWrite);
                if (GUILayout.Button("Generate Texture"))
                {
                    var tex = TextureGenerator.GenerateTexture("Assets", "texture", texParam);
                    Selection.activeObject = tex;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
