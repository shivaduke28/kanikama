using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.Baking
{
    [CreateAssetMenu(menuName = "Kanikama/UnityLightmapStorage", fileName = "UnityLightmapStorage")]
    public sealed class UnityLightmapStorageAsset : ScriptableObject
    {
        public const string DefaultFileName = "UnityLightmapStorage.asset";
        [SerializeField] UnityLightmapStorage storage = new UnityLightmapStorage();

        public UnityLightmapStorage Storage
        {
            get => storage;
            set => storage = value;
        }


        public static UnityLightmapStorageAsset FindOrCreate(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityLightmapStorageAsset>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = CreateInstance<UnityLightmapStorageAsset>();
            AssetDatabase.CreateAsset(asset, path);
            return AssetDatabase.LoadAssetAtPath<UnityLightmapStorageAsset>(path);
        }
    }
}
