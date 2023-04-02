using System;
using System.Linq;
using Kanikama.Core.Editor;
using Kanikama.Core.Editor.Textures;
using UnityEditor;
using UnityEngine;

namespace Kanikama.GI.Editor
{
    [Serializable]
    public sealed class BakingConfiguration
    {
        [SerializeField] SceneAsset sceneAsset;

        // [SerializeField, SpecifyObject(typeof(LightSource))]
        // SerializableGlobalObjectId[] lightSources;
        //
        // [SerializeField, SpecifyObject(typeof(LightSourceGroup))]
        // SerializableGlobalObjectId[] lightSourceGroups;

        [SerializeField] TextureResizeType textureResizeType;

        public SceneAsset SceneAsset => sceneAsset;
        public TextureResizeType TextureResizeType => textureResizeType;

        // public GlobalObjectId[] GetLightSources() => lightSources
        //     .Select(i => (Valid: i.TryParse(out var id), Id: id))
        //     .Where(t => t.Valid)
        //     .Select(t => t.Id).ToArray();
        //
        // public GlobalObjectId[] GetLightSourceGroups() => lightSourceGroups
        //     .Select(i => (Valid: i.TryParse(out var id), Id: id))
        //     .Where(t => t.Valid)
        //     .Select(t => t.Id).ToArray();


        public BakingConfiguration(SceneAsset sceneAsset,
            // SerializableGlobalObjectId[] lightSources,
            // SerializableGlobalObjectId[] lightSourceGroups,
            TextureResizeType textureResizeType)
        {
            this.sceneAsset = sceneAsset;
            // this.lightSources = lightSources;
            // this.lightSourceGroups = lightSourceGroups;
            this.textureResizeType = textureResizeType;
        }

        public BakingConfiguration Clone()
        {
            // var ls = new SerializableGlobalObjectId[lightSources.Length];
            // Array.Copy(lightSources, ls, lightSources.Length);
            // var lsg = new SerializableGlobalObjectId[lightSourceGroups.Length];
            // Array.Copy(lightSourceGroups, lsg, lightSourceGroups.Length);
            return new BakingConfiguration(sceneAsset, textureResizeType);
        }

        // public void UpdateSceneAsset(SceneAsset sceneAsset)
        // {
        //     var path = AssetDatabase.GetAssetPath(sceneAsset);
        //     var guid = AssetDatabase.AssetPathToGUID(path);
        //     this.sceneAsset = sceneAsset;
        //     for (var i = 0; i < lightSources.Length; i++)
        //     {
        //         var id = lightSources[i];
        //         id.assetGUID = guid;
        //         lightSources[i] = id;
        //     }
        //     for (var i = 0; i < lightSourceGroups.Length; i++)
        //     {
        //         var id = lightSourceGroups[i];
        //         id.assetGUID = guid;
        //         lightSourceGroups[i] = id;
        //     }
        // }
    }
}
