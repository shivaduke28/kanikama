using Kanikama.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Editor
{
    [CreateAssetMenu(menuName = "Kanikama/GI/BakingConfiguration", fileName = "KanikamaGIBakingConfiguration")]
    public sealed class BakingConfigurationAsset : ScriptableObject
    {
        [SerializeField] BakingConfiguration bakingConfiguration;
        public BakingConfiguration Configuration => bakingConfiguration;

        public static BakingConfigurationAsset Find(SceneAsset sceneAsset)
        {
            var assets = AssetDatabase.FindAssets($"t:{typeof(BakingConfigurationAsset)}");
            foreach (var asset in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(asset);
                var settings = AssetDatabase.LoadAssetAtPath<BakingConfigurationAsset>(path);
                if (settings.Configuration.SceneAsset == sceneAsset) return settings;
            }

            return null;
        }
    }

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

                var context = new BakingPipeline.BakingContext
                {
                    BakingConfiguration = config,
                    SceneAssetData = sceneAssetData,
                };

                var _ = BakingPipeline.BakeWithoutKanikamaAsync(context, default);
            }

            if (GUILayout.Button("Bake Kanikama"))
            {
                var config = asset.Configuration.Clone();
                var sceneAsset = config.SceneAsset;
                if (sceneAsset == null) return;

                var sceneAssetData = KanikamaSceneUtility.ToAssetData(sceneAsset);

                var context = new BakingPipeline.BakingContext
                {
                    BakingConfiguration = config,
                    SceneAssetData = sceneAssetData,
                };

                var _ = BakingPipeline.BakeAsync(context, default);
            }
        }
    }
}
