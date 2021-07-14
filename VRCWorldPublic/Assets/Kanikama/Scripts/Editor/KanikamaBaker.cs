using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Kanikama.EditorOnly;
using UnityEditor;
using System.IO;
using VRC.Udon;
using UdonSharp;
using System.Linq;
using Kanikama.Udon;
using UdonSharpEditor;
using System.Text.RegularExpressions;

namespace Kanikama.Editor
{
    public static class KanikamaBaker
    {
        [MenuItem("KanikamaGI/Debug")]
        public static void MyDebug()
        {
            Debug.Log(LightmapEditorSettings.maxAtlasSize);
        }

        [MenuItem("KanikamaGI/Bake")]
        public static void Bake()
        {
            var scene = SceneManager.GetActiveScene();

            var sceneData = new KanikamaSceneData();
            sceneData.LoadActiveScene();
            sceneData.SetupForBake();


            var sceneDirPath = Path.GetDirectoryName(scene.path);
            var kanikamaDirPath = Path.Combine(sceneDirPath, "Kanikama");
            AssetUtil.CreateFolderIfNecessary(sceneDirPath, "Kanikama");

            // 2d array generator
            var bakedAssetsDirPath = Path.Combine(sceneDirPath, scene.name.ToLower());

            // baked maps grouped by lightmapIndex
            var kanikamaLightmapDic = new Dictionary<int, List<Texture2D>>();


            // bake kanikama maps
            var lightmapAtlasCount = 0;
            for (var i = 0; i < sceneData.kanikamaLightData.Count; i++)
            {
                var light = sceneData.kanikamaLightData[i];
                light.Enabled = true;

                Lightmapping.Bake();

                if (i == 0) lightmapAtlasCount = GetBakedLightmapCount(bakedAssetsDirPath);
                for (var lmInd = 0; lmInd < lightmapAtlasCount; lmInd++)
                {
                    var bakedMapPath = Path.Combine(bakedAssetsDirPath, $"Lightmap-{lmInd}_comp_light.exr");
                    var copiedMapPath = Path.Combine(kanikamaDirPath, $"KLight-{lmInd}_{i}.exr");
                    AssetDatabase.CopyAsset(bakedMapPath, copiedMapPath);

                    var textureImporter = AssetImporter.GetAtPath(copiedMapPath) as TextureImporter;
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();

                    if (!kanikamaLightmapDic.ContainsKey(lmInd))
                    {
                        kanikamaLightmapDic[lmInd] = new List<Texture2D>();
                    }

                    kanikamaLightmapDic[lmInd].Add(AssetDatabase.LoadAssetAtPath<Texture2D>(copiedMapPath));
                }

                light.Enabled = false;
            }


            AssetDatabase.Refresh();

            // generate tex2d arrays, composite materials, custom render textures
            for (var lmInd = 0; lmInd < lightmapAtlasCount; lmInd++)
            {

                var texArr = Texture2DArrayGenerator.Generate(kanikamaLightmapDic[lmInd]);
                AssetUtil.CreateOrReplaceAsset(ref texArr, Path.Combine(kanikamaDirPath, $"KLightArr-{lmInd}.asset"));

                var shader = Shader.Find("Kanikama/LightmapComposite");
                var mat = new Material(shader);
                var matPath = Path.Combine(kanikamaDirPath, $"KLightComposite-{lmInd}.mat");
                AssetUtil.CreateOrReplaceAsset(ref mat, matPath);

                mat.SetInt("_TexCount", sceneData.kanikamaLightData.Count);
                mat.SetTexture("_Tex2DArray", texArr);

                var sceneMapPath = Path.Combine(kanikamaDirPath, $"KLightComposite-{lmInd}.asset");
                var sceneMap = new CustomRenderTexture(texArr.width, texArr.height, RenderTextureFormat.ARGB32)
                {
                    material = mat,
                    updateMode = CustomRenderTextureUpdateMode.Realtime,
                    initializationMode = CustomRenderTextureUpdateMode.OnLoad,
                    initializationColor = Color.black
                };
                AssetUtil.CreateOrReplaceAsset(ref sceneMap, sceneMapPath);
            }

            AssetDatabase.Refresh();



            // monitor
            var monitorMapsDic = new Dictionary<int, Dictionary<int, List<Texture2D>>>();

            for (var m = 0; m < sceneData.sceneDescriptor.kanikamaMonitors.Count; m++)
            {
                var dic = new Dictionary<int, List<Texture2D>>();
                monitorMapsDic[m] = dic;
                var monitor = sceneData.sceneDescriptor.kanikamaMonitors[m];
                for (var i = 0; i < monitor.lights.Count; i++)
                {
                    var light = monitor.lights[i];
                    light.enabled = true;

                    Lightmapping.Bake();

                    if (lightmapAtlasCount == 0) lightmapAtlasCount = GetBakedLightmapCount(bakedAssetsDirPath);

                    for (var lmInd = 0; lmInd < lightmapAtlasCount; lmInd++)
                    {
                        var bakedMapPath = Path.Combine(bakedAssetsDirPath, $"Lightmap-{lmInd}_comp_light.exr");
                        var copiedMapPath = Path.Combine(kanikamaDirPath, $"KMonitor-{m}-{lmInd}_{i}.exr");
                        AssetDatabase.CopyAsset(bakedMapPath, copiedMapPath);

                        var textureImporter = AssetImporter.GetAtPath(copiedMapPath) as TextureImporter;
                        textureImporter.isReadable = true;
                        textureImporter.SaveAndReimport();

                        if (!dic.ContainsKey(lmInd))
                        {
                            dic[lmInd] = new List<Texture2D>();
                        }

                        dic[lmInd].Add(AssetDatabase.LoadAssetAtPath<Texture2D>(copiedMapPath));
                    }

                    light.enabled = false;
                }
            }

            // generate tex2d arrays, composite materials, custom render textures
            for (var lmInd = 0; lmInd < lightmapAtlasCount; lmInd++)
            {

                var texArr = Texture2DArrayGenerator.Generate(monitorMapsDic[0][lmInd]);
                AssetUtil.CreateOrReplaceAsset(ref texArr, Path.Combine(kanikamaDirPath, $"KMonitorArr-{lmInd}.asset"));

                var shader = Shader.Find("Kanikama/LightmapComposite");
                var mat = new Material(shader);
                var matPath = Path.Combine(kanikamaDirPath, $"KMonitorComposite-{lmInd}.mat");
                AssetUtil.CreateOrReplaceAsset(ref mat, matPath);

                mat.SetInt("_TexCount", sceneData.sceneDescriptor.kanikamaMonitors[0].lights.Count);
                mat.SetTexture("_Tex2DArray", texArr);

                var sceneMapPath = Path.Combine(kanikamaDirPath, $"KMonitorComposite-{lmInd}.asset");
                var sceneMap = new CustomRenderTexture(texArr.width, texArr.height, RenderTextureFormat.ARGB32)
                {
                    material = mat,
                    updateMode = CustomRenderTextureUpdateMode.Realtime,
                    initializationMode = CustomRenderTextureUpdateMode.OnLoad,
                    initializationColor = Color.black
                };
                AssetUtil.CreateOrReplaceAsset(ref sceneMap, sceneMapPath);
            }

            AssetDatabase.Refresh();




            sceneData.Rollback();

            // bake scene lightmaps without kanikama
            Lightmapping.Bake();
        }

        private static readonly Regex LightMapRegex = new Regex("Lightmap-[0-9]+_comp_light.exr");

        private static int GetBakedLightmapCount(string bakedAssetsDirPath)
        {
            return AssetDatabase.FindAssets("t:Texture", new string[1] { bakedAssetsDirPath })
                .Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Where(x => LightMapRegex.IsMatch(x)).Count();
        }
    }
}