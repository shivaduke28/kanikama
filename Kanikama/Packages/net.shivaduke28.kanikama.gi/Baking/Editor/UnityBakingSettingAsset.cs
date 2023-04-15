using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Editor
{
    [CreateAssetMenu(menuName = "Kanikama/GI/BakingConfiguration", fileName = "KanikamaGIBakingConfiguration")]
    public sealed class UnityBakingSettingAsset : ScriptableObject
    {
        [SerializeField] UnityBakingSetting unityBakingSetting;
        public UnityBakingSetting Setting => unityBakingSetting;

        public static UnityBakingSettingAsset Find(SceneAsset sceneAsset)
        {
            var assets = AssetDatabase.FindAssets($"t:{typeof(UnityBakingSettingAsset)}");
            foreach (var asset in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(asset);
                var settings = AssetDatabase.LoadAssetAtPath<UnityBakingSettingAsset>(path);
                if (settings.Setting.SceneAsset == sceneAsset) return settings;
            }

            return null;
        }
    }
}
