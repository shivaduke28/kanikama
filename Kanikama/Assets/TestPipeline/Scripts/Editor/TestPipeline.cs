using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kanikama.Core;
using Kanikama.Core.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Test.Editor
{
    public class TestPipeline
    {
        [MenuItem("Kanikama/Execute Test Pipeline")]
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
                var lightReference = new ComponentReference<Light>(light);

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
                var sceneLightingCollection = KanikamaSceneUtility.GetSceneLightingCollection();
                sceneLightingCollection.TurnOff();
                sceneLightingCollection.DisableLightProbes();
                sceneLightingCollection.DisableReflectionProbes();

                // non directional
                LightmapEditorSettings.lightmapsMode = LightmapsMode.NonDirectional;

                var context = new Context
                {
                    Light = light,
                    Original = sceneAssetData,
                    Copied = copiedSceneAssetData,
                    Lightmapper = new Lightmapper(),
                    DstDir = $"{sceneAssetData.LightingAssetDirectoryPath}_dst"
                };

                KanikamaSceneUtility.CreateFolderIfNecessary(context.DstDir);

                var indirects = await BakeIndirectAsync(context);
                var directs = await BakeDirectAsync(context);
                var mixed = await BakeMixedAsync(context);


                for (var i = 0; i < indirects.Count; i++)
                {
                    var indirect = indirects[i].Texture;
                    var direct = directs[i].Texture;
                    var rt = KanikamaTextureUtility.Subtract(indirect, direct, indirect.width, indirect.height);
                    var compressed = KanikamaTextureUtility.CompressToBC6H(rt, false, true, TextureCompressionQuality.Best);

                    var path = Path.Combine(context.DstDir, $"subtract_{indirects[i].Index}.asset");
                    KanikamaSceneUtility.CreateOrReplaceAsset(ref compressed, path);
                }


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
        }

        static async ValueTask<List<BakedLightmap>> BakeIndirectAsync(Context context)
        {
            context.Light.lightmapBakeType = LightmapBakeType.Baked;
            context.Light.bounceIntensity = 1;
            Lightmapping.ClearDiskCache();
            await context.Lightmapper.BakeAsync(default);
            var bakedLightingAssetCollection = KanikamaSceneUtility.GetBakedLightingAssetCollection(context.Copied);


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

            return result;
        }

        static async ValueTask<List<BakedLightmap>> BakeDirectAsync(Context context)
        {
            context.Light.lightmapBakeType = LightmapBakeType.Baked;
            context.Light.bounceIntensity = 0;

            Lightmapping.ClearDiskCache();
            await context.Lightmapper.BakeAsync(default);
            var bakedLightingAssetCollection = KanikamaSceneUtility.GetBakedLightingAssetCollection(context.Copied);

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

            return result;
        }

        static async ValueTask<List<BakedLightmap>> BakeMixedAsync(Context context)
        {
            context.Light.lightmapBakeType = LightmapBakeType.Mixed;
            context.Light.bounceIntensity = 1;

            Lightmapping.ClearDiskCache();
            await context.Lightmapper.BakeAsync(default);
            var bakedLightingAssetCollection = KanikamaSceneUtility.GetBakedLightingAssetCollection(context.Copied);

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

            return result;
        }
    }
}
