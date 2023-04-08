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
}
