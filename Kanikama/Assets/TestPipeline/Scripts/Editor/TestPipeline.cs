using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kanikama.Core.Editor;
using Kanikama.Core.Editor.Util;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Test.Editor
{
    public class TestPipeline
    {
        [MenuItem("Kanikama/Test/Execute Test Pipeline")]
        public static void Bake()
        {
            _ = ExecuteAsync();
        }

        static async Task ExecuteAsync()
        {
            // make sure the current active scene is saved as a SceneAsset
            if (!SceneAssetData.TryFindFromActiveScene(out var sceneAssetData)) return;

            // get objects that you want to control in this pipeline.
            var myLightReference = Object.FindObjectOfType<MyLightReference>();
            if (myLightReference == null) return;


            // create copy
            using (var copiedSceneAsset = CopiedSceneAsset.Create(sceneAssetData, true))
            {
                // create a reference before change your scene
                var light = myLightReference.Light;
                var lightReference = new ObjectHandle<Light>(light);

                var copiedSceneAssetData = copiedSceneAsset.SceneAssetData;

                // change to the copied scene
                EditorSceneManager.OpenScene(copiedSceneAsset.SceneAssetData.Path);

                // get a reference in the copied scene (using hierarchy path)
                light = lightReference.Value;
                light.color = Color.white;
                light.intensity = 1;
                light.bounceIntensity = 1;
                light.lightmapBakeType = LightmapBakeType.Baked;

                // turn off other light sources
                var sceneLightingCollection = UnitySceneGIContext.GetGIContext();
                sceneLightingCollection.TurnOff();

                // non directional
                LightmapEditorSettings.lightmapsMode = LightmapsMode.NonDirectional;


                var dstDir = $"{sceneAssetData.LightingAssetDirectoryPath}_dst";
                IOUtility.CreateFolderIfNecessary(dstDir);
                var bakedAssetDataBase = new UnityLightmapStorage();

                var context = new Context
                {
                    Light = light,
                    Original = sceneAssetData,
                    Copied = copiedSceneAssetData,
                    UnityLightmapper = new UnityLightmapper(),
                    DstDir = dstDir,
                    UnityLightmapStorage = bakedAssetDataBase,
                };


                await BakeIndirectAsync(context);
                await BakeDirectAsync(context);
                await BakeMixedAsync(context);

                bakedAssetDataBase.TryGet("indirect", out var indirects);
                bakedAssetDataBase.TryGet("direct", out var directs);

                var indirectLightmaps = indirects;
                var directLightmaps = directs;

                for (var i = 0; i < indirectLightmaps.Count; i++)
                {
                    var indirect = indirectLightmaps[i].Texture;
                    var direct = directLightmaps[i].Texture;
                    var rt = TextureUtility.Subtract(indirect, direct, indirect.width, indirect.height);
                    var compressed = TextureUtility.CompressToBC6H(rt, false, true, TextureCompressionQuality.Best);

                    var path = Path.Combine(context.DstDir, $"subtract_{indirectLightmaps[i].Index}.asset");
                    IOUtility.CreateOrReplaceAsset(ref compressed, path);
                }

                var bakedAssetRepository = UnityLightmapStorageAsset.FindOrCreate(Path.Combine(dstDir, UnityLightmapStorageAsset.DefaultFileName));
                bakedAssetRepository.Storage = bakedAssetDataBase;
                EditorUtility.SetDirty(bakedAssetRepository);
                AssetDatabase.SaveAssets();


                // delete copied scene and generated lightmaps
            }

            // back to the original scene
            EditorSceneManager.OpenScene(sceneAssetData.Path);
        }

        class Context
        {
            public Light Light;
            public UnityLightmapper UnityLightmapper;
            public SceneAssetData Original;
            public SceneAssetData Copied;
            public string DstDir;
            public UnityLightmapStorage UnityLightmapStorage;
        }

        static async Task BakeIndirectAsync(Context context)
        {
            context.Light.lightmapBakeType = LightmapBakeType.Baked;
            context.Light.bounceIntensity = 1;
            Lightmapping.ClearDiskCache();
            await context.UnityLightmapper.BakeAsync(default);
            var bakedLightingAssetCollection = UnityLightmapUtility.GetLightmaps(context.Copied);


            // copy baked lightmaps
            string RenameFunc(UnityLightmap bakedLightmap)
            {
                var ext = Path.GetExtension(bakedLightmap.Path);
                var fileName = $"indirect_{bakedLightmap.Type}_{bakedLightmap.Index}{ext}";
                return Path.Combine(context.DstDir, fileName);
            }

            var result = new List<UnityLightmap>();

            foreach (var bakedLightmap in bakedLightingAssetCollection)
            {
                var copied = UnityLightmapUtility.CopyBakedLightmap(bakedLightmap, RenameFunc(bakedLightmap));
                result.Add(copied);
            }

            context.UnityLightmapStorage.AddOrUpdate("indirect", result);
        }

        static async Task BakeDirectAsync(Context context)
        {
            context.Light.lightmapBakeType = LightmapBakeType.Baked;
            context.Light.bounceIntensity = 0;

            Lightmapping.ClearDiskCache();
            await context.UnityLightmapper.BakeAsync(default);
            var bakedLightingAssetCollection = UnityLightmapUtility.GetLightmaps(context.Copied);

            string RenameFunc(UnityLightmap bakedLightmap)
            {
                var ext = Path.GetExtension(bakedLightmap.Path);
                var fileName = $"direct_{bakedLightmap.Type}_{bakedLightmap.Index}{ext}";
                return Path.Combine(context.DstDir, fileName);
            }

            var result = new List<UnityLightmap>();

            foreach (var bakedLightmap in bakedLightingAssetCollection)
            {
                var copied = UnityLightmapUtility.CopyBakedLightmap(bakedLightmap, RenameFunc(bakedLightmap));
                result.Add(copied);
            }

            context.UnityLightmapStorage.AddOrUpdate("direct", result);
        }

        static async Task BakeMixedAsync(Context context)
        {
            context.Light.lightmapBakeType = LightmapBakeType.Mixed;
            context.Light.bounceIntensity = 1;

            Lightmapping.ClearDiskCache();
            await context.UnityLightmapper.BakeAsync(default);
            var bakedLightingAssetCollection = UnityLightmapUtility.GetLightmaps(context.Copied);

            string RenameFunc(UnityLightmap bakedLightmap)
            {
                var ext = Path.GetExtension(bakedLightmap.Path);
                var fileName = $"mixed_{bakedLightmap.Type}_{bakedLightmap.Index}{ext}";
                return Path.Combine(context.DstDir, fileName);
            }

            var result = new List<UnityLightmap>();

            foreach (var bakedLightmap in bakedLightingAssetCollection)
            {
                var copied = UnityLightmapUtility.CopyBakedLightmap(bakedLightmap, RenameFunc(bakedLightmap));
                result.Add(copied);
            }

            context.UnityLightmapStorage.AddOrUpdate("mixed", result);
        }
    }
}
