using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kanikama.Core;
using Kanikama.Core.Editor;
using Kanikama.Core.Editor.Textures;
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
            if (!KanikamaSceneUtility.TryGetActiveSceneAsset(out var sceneAssetData)) return;

            // get objects that you want to control in this pipeline.
            var myLightReference = Object.FindObjectOfType<MyLightReference>();
            if (myLightReference == null) return;


            // create copy
            using (var copiedSceneHandler = KanikamaSceneUtility.CopySceneAsset(sceneAssetData))
            {
                // create a reference before change your scene
                var light = myLightReference.Light;
                var lightReference = new ObjectHandle<Light>(light);

                var copiedSceneAssetData = copiedSceneHandler.SceneAssetData;

                // change to the copied scene
                EditorSceneManager.OpenScene(copiedSceneHandler.SceneAssetData.Path);

                // get a reference in the copied scene (using hierarchy path)
                light = lightReference.Value;
                light.color = Color.white;
                light.intensity = 1;
                light.bounceIntensity = 1;
                light.lightmapBakeType = LightmapBakeType.Baked;

                // turn off other light sources
                var sceneLightingCollection = KanikamaSceneUtility.GetSceneGIContext();
                sceneLightingCollection.TurnOff();
                sceneLightingCollection.DisableLightProbes();
                sceneLightingCollection.DisableReflectionProbes();

                // non directional
                LightmapEditorSettings.lightmapsMode = LightmapsMode.NonDirectional;


                var dstDir = $"{sceneAssetData.LightingAssetDirectoryPath}_dst";
                KanikamaSceneUtility.CreateFolderIfNecessary(dstDir);
                var bakedAssetDataBase = new BakedAssetDataBase();

                var context = new Context
                {
                    Light = light,
                    Original = sceneAssetData,
                    Copied = copiedSceneAssetData,
                    Lightmapper = new Lightmapper(),
                    DstDir = dstDir,
                    BakedAssetDataBase = bakedAssetDataBase,
                };


                await BakeIndirectAsync(context);
                await BakeDirectAsync(context);
                await BakeMixedAsync(context);

                bakedAssetDataBase.TryGet("indirect", out var indirects);
                bakedAssetDataBase.TryGet("direct", out var directs);

                var indirectLightmaps = indirects.Lightmaps;
                var directLightmaps = directs.Lightmaps;

                for (var i = 0; i < indirectLightmaps.Count; i++)
                {
                    var indirect = indirectLightmaps[i].Texture;
                    var direct = directLightmaps[i].Texture;
                    var rt = TextureUtility.Subtract(indirect, direct, indirect.width, indirect.height);
                    var compressed = TextureUtility.CompressToBC6H(rt, false, true, TextureCompressionQuality.Best);

                    var path = Path.Combine(context.DstDir, $"subtract_{indirectLightmaps[i].Index}.asset");
                    KanikamaSceneUtility.CreateOrReplaceAsset(ref compressed, path);
                }

                var bakedAssetRepository = BakedAssetRepository.FindOrCreate(Path.Combine(dstDir, BakedAssetRepository.DefaultFileName));
                bakedAssetRepository.DataBase = bakedAssetDataBase;
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
            public Lightmapper Lightmapper;
            public SceneAssetData Original;
            public SceneAssetData Copied;
            public string DstDir;
            public BakedAssetDataBase BakedAssetDataBase;
        }

        static async Task BakeIndirectAsync(Context context)
        {
            context.Light.lightmapBakeType = LightmapBakeType.Baked;
            context.Light.bounceIntensity = 1;
            Lightmapping.ClearDiskCache();
            await context.Lightmapper.BakeAsync(default);
            var bakedLightingAssetCollection = KanikamaSceneUtility.GetBakedAssetData(context.Copied);


            // copy baked lightmaps
            string RenameFunc(BakedLightmap bakedLightmap)
            {
                var ext = Path.GetExtension(bakedLightmap.Path);
                var fileName = $"indirect_{bakedLightmap.Type}_{bakedLightmap.Index}{ext}";
                return Path.Combine(context.DstDir, fileName);
            }

            var result = new List<BakedLightmap>();

            foreach (var bakedLightmap in bakedLightingAssetCollection.Lightmaps)
            {
                var copied = KanikamaSceneUtility.CopyBakedLightmap(bakedLightmap, RenameFunc(bakedLightmap));
                result.Add(copied);
            }

            context.BakedAssetDataBase.AddOrUpdate("indirect", new BakedLightingAssetCollection { Lightmaps = result });
        }

        static async Task BakeDirectAsync(Context context)
        {
            context.Light.lightmapBakeType = LightmapBakeType.Baked;
            context.Light.bounceIntensity = 0;

            Lightmapping.ClearDiskCache();
            await context.Lightmapper.BakeAsync(default);
            var bakedLightingAssetCollection = KanikamaSceneUtility.GetBakedAssetData(context.Copied);

            string RenameFunc(BakedLightmap bakedLightmap)
            {
                var ext = Path.GetExtension(bakedLightmap.Path);
                var fileName = $"direct_{bakedLightmap.Type}_{bakedLightmap.Index}{ext}";
                return Path.Combine(context.DstDir, fileName);
            }

            var result = new List<BakedLightmap>();

            foreach (var bakedLightmap in bakedLightingAssetCollection.Lightmaps)
            {
                var copied = KanikamaSceneUtility.CopyBakedLightmap(bakedLightmap, RenameFunc(bakedLightmap));
                result.Add(copied);
            }

            context.BakedAssetDataBase.AddOrUpdate("direct", new BakedLightingAssetCollection { Lightmaps = result });
        }

        static async Task BakeMixedAsync(Context context)
        {
            context.Light.lightmapBakeType = LightmapBakeType.Mixed;
            context.Light.bounceIntensity = 1;

            Lightmapping.ClearDiskCache();
            await context.Lightmapper.BakeAsync(default);
            var bakedLightingAssetCollection = KanikamaSceneUtility.GetBakedAssetData(context.Copied);

            string RenameFunc(BakedLightmap bakedLightmap)
            {
                var ext = Path.GetExtension(bakedLightmap.Path);
                var fileName = $"mixed_{bakedLightmap.Type}_{bakedLightmap.Index}{ext}";
                return Path.Combine(context.DstDir, fileName);
            }

            var result = new List<BakedLightmap>();

            foreach (var bakedLightmap in bakedLightingAssetCollection.Lightmaps)
            {
                var copied = KanikamaSceneUtility.CopyBakedLightmap(bakedLightmap, RenameFunc(bakedLightmap));
                result.Add(copied);
            }

            context.BakedAssetDataBase.AddOrUpdate("mixed", new BakedLightingAssetCollection { Lightmaps = result });
        }
    }
}
