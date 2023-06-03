using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.Baking.GUI
{
    internal sealed class TextureUtilityDrawer : KanikamaWindow.IGUIDrawer
    {
        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Others, () => new TextureUtilityDrawer(), 1);
        }

        readonly TextureParameter textureParameter = new TextureParameter();
        readonly TextureImportParameter textureImportParameter = new TextureImportParameter();
        string outputDir = "Assets";
        string fileName = "texture";

        TextureUtilityDrawer()
        {
        }

        void KanikamaWindow.IGUIDrawer.OnLoadActiveScene()
        {
        }

        void KanikamaWindow.IGUIDrawer.Draw()
        {
            EditorGUILayout.LabelField("Texture Utility", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                DrawTextureParameter(textureParameter);
                EditorGUILayout.Space();
                DrawTextureImportParameter(textureImportParameter);
                EditorGUILayout.Space();
                outputDir = EditorGUILayout.TextField("Output Directory", outputDir);
                fileName = EditorGUILayout.TextField("File Name", fileName);
                EditorGUILayout.Space();
                if (KanikamaGUI.Button("Create"))
                {
                    var tex = TextureUtility.GenerateTexture(textureParameter);
                    TextureUtility.SaveTexture2D(tex, outputDir, fileName, textureImportParameter);
                    Selection.activeObject = tex;
                }
            }
        }

        static void DrawTextureParameter(TextureParameter textureParameter)
        {
            EditorGUILayout.LabelField("Texture Parameter", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                textureParameter.Width = EditorGUILayout.IntField(nameof(textureParameter.Width), textureParameter.Width);
                textureParameter.Height = EditorGUILayout.IntField(nameof(textureParameter.Height), textureParameter.Height);
                textureParameter.Format = (TextureFormat) EditorGUILayout.EnumPopup(nameof(textureParameter.Width), textureParameter.Format);
                textureParameter.MipChain = EditorGUILayout.Toggle(nameof(textureParameter.MipChain), textureParameter.MipChain);
                textureParameter.Linear = EditorGUILayout.Toggle(nameof(textureParameter.Linear), textureParameter.Linear);
            }
        }

        static void DrawTextureImportParameter(TextureImportParameter textureImportParameter)
        {
            EditorGUILayout.LabelField("Texture Parameter", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                textureImportParameter.Extension =
                    (TextureExtension) EditorGUILayout.EnumPopup(nameof(textureImportParameter.Extension), textureImportParameter.Extension);
                textureImportParameter.IsReadable =
                    EditorGUILayout.Toggle(nameof(textureImportParameter.IsReadable), textureImportParameter.IsReadable);
                textureImportParameter.Compression =
                    (TextureImporterCompression) EditorGUILayout.EnumPopup(nameof(textureImportParameter.Compression), textureImportParameter.Compression);
                textureImportParameter.CompressedFormat =
                    (TextureFormat) EditorGUILayout.EnumPopup(nameof(textureImportParameter.CompressedFormat), textureImportParameter.CompressedFormat);
                textureImportParameter.CompressionQuality =
                    (TextureCompressionQuality) EditorGUILayout.EnumPopup(nameof(textureImportParameter.CompressionQuality),
                        textureImportParameter.CompressionQuality);
            }
        }
    }
}
