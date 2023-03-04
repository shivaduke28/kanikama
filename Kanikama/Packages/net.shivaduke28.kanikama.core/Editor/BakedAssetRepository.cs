using UnityEditor;
using UnityEngine;

namespace Kanikama.Core.Editor
{
    [CreateAssetMenu(menuName = "Kanikama/BakedAssetRepository", fileName = "BakedAssetRepository")]
    public sealed class BakedAssetRepository : ScriptableObject
    {
        public const string DefaultFileName = "BakedAssetRepository.asset";
        [SerializeField] BakedAssetDataBase dataBase = new BakedAssetDataBase();
        
        public BakedAssetDataBase DataBase
        {
            get => dataBase;
            set => dataBase = value;
        }


        public static BakedAssetRepository FindOrCreate(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<BakedAssetRepository>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = CreateInstance<BakedAssetRepository>();
            AssetDatabase.CreateAsset(asset, path);
            return AssetDatabase.LoadAssetAtPath<BakedAssetRepository>(path);
        }
    }
}
