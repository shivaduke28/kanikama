using System.Collections.Generic;
using Kanikama.Editor.Baking;
using Kanikama.Utility;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Bakery.Editor.Baking
{
    public static class KanikamaBakeryUtility
    {
        public static List<Lightmap> GetLightmaps()
        {
            var result = new List<Lightmap>();
            var ftLightmapsStorage = GameObjectHelper.FindObjectOfType<ftLightmapsStorage>();
            if (ftLightmapsStorage == null)
            {
                Debug.LogWarningFormat(KanikamaDebug.Format, "ftLightmapStorage is not found.");
                return result;
            }

            var lightmaps = ftLightmapsStorage.maps;
            if (lightmaps != null)
            {
                for (var i = 0; i < lightmaps.Count; i++)
                {
                    var texture = lightmaps[i];
                    var path = AssetDatabase.GetAssetPath(texture);
                    result.Add(new Lightmap(BakeryLightmap.Light, texture, path, i));
                }
            }

            var dirMaps = ftLightmapsStorage.dirMaps;
            if (dirMaps != null)
            {
                for (var i = 0; i < dirMaps.Count; i++)
                {
                    var texture = dirMaps[i];
                    var path = AssetDatabase.GetAssetPath(texture);
                    result.Add(new Lightmap(BakeryLightmap.Directional, texture, path, i));
                }
            }
            return result;
        }
    }
}
