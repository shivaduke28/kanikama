using Kanikama.Baking;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor
{
    [CreateAssetMenu(menuName = "Kanikama/Settings", fileName = "KanikamaSettings")]
    public class KanikamaSettings : ScriptableObject
    {
        [SerializeField] SceneAsset sceneAsset;
        public LightmapperType lightmapperType;
        public bool directionalMode;
        public bool createRenderTexture;
        public bool createCustomRenderTexture;
        public bool packTextures;
        public BakedAsset bakedAsset;

        public SceneAsset SceneAsset => sceneAsset;

        public void Initialize(SceneAsset sceneAsset, bool directionalMode)
        {
            this.sceneAsset = sceneAsset;
            this.directionalMode = directionalMode;
        }

        public void UpdateAsset(BakedAsset asset)
        {
            bakedAsset.RemoveNullAssets();
            bakedAsset.kanikamaMapArrays = new List<Texture2DArray>(asset.kanikamaMapArrays);
            if (directionalMode)
            {
                bakedAsset.kanikamaDirectionalMapArrays = new List<Texture2DArray>(asset.kanikamaDirectionalMapArrays);
            }

            if (createCustomRenderTexture)
            {
                bakedAsset.customRenderTextures = new List<CustomRenderTexture>(asset.customRenderTextures);
                bakedAsset.customRenderTextureMaterials = new List<Material>(asset.customRenderTextureMaterials);
            }

            if (createRenderTexture)
            {
                bakedAsset.renderTextures = new List<RenderTexture>(asset.renderTextures);
                bakedAsset.renderTextureMaterials = new List<Material>(asset.renderTextureMaterials);
            }

            bakedAsset.sliceCount = asset.sliceCount;
        }

        public static KanikamaSettings FindOrCreateSettings(SceneAsset sceneAsset)
        {
            var settings = FindSettings(sceneAsset);

            if (settings != null) return settings;

            settings = CreateInstance<KanikamaSettings>();
            settings.Initialize(sceneAsset, LightmapEditorSettings.lightmapsMode == LightmapsMode.CombinedDirectional);
            var dirPath = KanikamaPath.KanikamaAssetDirPath(sceneAsset);
            AssetDatabase.CreateAsset(settings, Path.Combine(dirPath, "KanikamaSettings.asset"));
            AssetDatabase.Refresh();
            return settings;
        }

        public static KanikamaSettings FindSettings(SceneAsset sceneAsset)
        {
            var assets = AssetDatabase.FindAssets($"t:{typeof(KanikamaSettings)}");
            foreach (var asset in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(asset);
                var settings = AssetDatabase.LoadAssetAtPath<KanikamaSettings>(path);
                if (settings.SceneAsset == sceneAsset) return settings;
            }

            return null;
        }
    }
}