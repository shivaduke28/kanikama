using System.IO;
using Kanikama.Core.Editor;
using Kanikama.GI.Baking;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Editor.GUI
{
    [CustomEditor(typeof(UnityBakingSettingAsset))]
    internal sealed class UntiyBakingSettingAssetEditor : UnityEditor.Editor
    {
        UnityBakingSettingAsset asset;

        void OnEnable()
        {
            asset = (UnityBakingSettingAsset) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (GUILayout.Button("Bake without Kanikama"))
            {
                var config = asset.Setting.Clone();
                var sceneAsset = config.SceneAsset;
                if (sceneAsset == null) return;

                var sceneAssetData = KanikamaSceneUtility.ToAssetData(sceneAsset);

                var baking = KanikamaSceneUtility.FindObjectOfType<IBakingDescriptor>();
                if (baking != null)
                {
                    var _ = UnityBakingPipelineRunner.RunWithoutKanikamaAsync(baking, sceneAssetData, default);
                    return;
                }
            }

            if (GUILayout.Button("Bake Kanikama"))
            {
                var config = asset.Setting.Clone();
                var sceneAsset = config.SceneAsset;
                if (sceneAsset == null) return;

                var sceneAssetData = KanikamaSceneUtility.ToAssetData(sceneAsset);

                var baking = KanikamaSceneUtility.FindObjectOfType<IBakingDescriptor>();
                if (baking != null)
                {
                    var _ = UnityBakingPipelineRunner.RunAsync(baking, sceneAssetData, default);
                }
            }

            if (GUILayout.Button("Create Assets"))
            {
                var config = asset.Setting.Clone();
                var sceneAsset = config.SceneAsset;
                if (sceneAsset == null) return;

                UnityBakingPipeline.CreateAssets(asset.Setting.LightmapStorage, asset.Setting.OutputAssetDirPath, config.TextureResizeType);
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
