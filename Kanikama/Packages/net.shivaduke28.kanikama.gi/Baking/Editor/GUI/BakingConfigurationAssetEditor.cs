using System.IO;
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


        void OnEnable()
        {
            asset = (BakingConfigurationAsset) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

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
                    var _ = BakingPipelineRunner.RunWithoutKanikamaAsync(baking, config, sceneAssetData, default);
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
                    var _ = BakingPipelineRunner.RunAsync(baking, config, sceneAssetData, default);
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
