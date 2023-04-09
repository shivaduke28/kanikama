using System.IO;
using System.Linq;
using Kanikama.Core.Editor;
using Kanikama.GI.Baking;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Editor.GUI
{
    [CustomEditor(typeof(BakingConfigurationAsset))]
    internal sealed class BakingConfigurationAssetEditor : UnityEditor.Editor
    {
        BakingConfigurationAsset asset;
        string[] lightmapperKeys;
        int lightmapperIndex;
        SerializedProperty lightmapperKey;

        void OnEnable()
        {
            asset = (BakingConfigurationAsset) target;
            lightmapperKeys = LightmapperFactory.GetKeys();
            var bakingConfiguration = serializedObject.FindProperty("bakingConfiguration");
            lightmapperKey = bakingConfiguration.FindPropertyRelative("lightmapperKey");
            var key = lightmapperKey.stringValue;
            for (var i = 0; i < lightmapperKeys.Length; i++)
            {
                if (key == lightmapperKeys[i])
                {
                    lightmapperIndex = i;
                    break;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                lightmapperIndex = EditorGUILayout.Popup("Lightmapper", lightmapperIndex, lightmapperKeys);

                if (check.changed)
                {
                    lightmapperKey.stringValue = lightmapperKeys[lightmapperIndex];
                    serializedObject.ApplyModifiedProperties();
                }
            }


            EditorGUILayout.Space();

            if (GUILayout.Button("Bake without Kanikama"))
            {
                var config = asset.Configuration.Clone();
                var sceneAsset = config.SceneAsset;
                if (sceneAsset == null) return;

                var sceneAssetData = KanikamaSceneUtility.ToAssetData(sceneAsset);

                var baking = KanikamaSceneUtility.FindObjectOfType<IBakingDescriptor>();
                if (baking != null)
                {
                    var _ = BakingPipelineRunner.RunWithoutKanikamaAsync(baking, sceneAssetData, config.LightmapperKey, default);
                    return;
                }
            }

            if (GUILayout.Button("Bake Kanikama"))
            {
                var config = asset.Configuration.Clone();
                var sceneAsset = config.SceneAsset;
                if (sceneAsset == null) return;

                var sceneAssetData = KanikamaSceneUtility.ToAssetData(sceneAsset);

                var baking = KanikamaSceneUtility.FindObjectOfType<IBakingDescriptor>();
                if (baking != null)
                {
                    var _ = BakingPipelineRunner.RunAsync(baking, sceneAssetData, config.LightmapperKey, default);
                }
            }

            if (GUILayout.Button("Create Assets"))
            {
                var config = asset.Configuration.Clone();
                var sceneAsset = config.SceneAsset;
                if (sceneAsset == null) return;

                var sceneAssetData = KanikamaSceneUtility.ToAssetData(sceneAsset);

                var dstDir = $"{sceneAssetData.LightingAssetDirectoryPath}_kanikama-temp";
                KanikamaSceneUtility.CreateFolderIfNecessary(dstDir);

                var bakedAssetRegistry = BakedAssetRepository.FindOrCreate(Path.Combine(dstDir, BakedAssetRepository.DefaultFileName));
                BakingPipeline.CreateAssets(bakedAssetRegistry.DataBase, $"{sceneAssetData.LightingAssetDirectoryPath}_kanikama-out", config.TextureResizeType);
                EditorUtility.SetDirty(bakedAssetRegistry);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
